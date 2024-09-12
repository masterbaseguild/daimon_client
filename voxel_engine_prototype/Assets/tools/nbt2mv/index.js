var fs = require('fs')
var zlib = require('zlib')
var nbt = require('nbt')

var folder = '../world/region/'

var regions = fs.readdirSync(folder)
var chunks = []
var cursor = 0;
regions.forEach(file=>{
    console.log('now editing '+file)
        parseRegion(folder, file, result => {
            chunks = chunks.concat(result)
            if(cursor==regions.length-1)
                {
                    chunks = JSON.stringify(chunks, null, '\t')
                    fs.writeFileSync('chunks.json', chunks)
                }
            else
                cursor++
        })
})

function parseRegion(folder, filename, callback)
{
    const unit = 4096;
    const file = fs.readFileSync((folder+filename))
    const regionCoords = filename.split('.')
    regionCoords.pop()
    regionCoords.shift()
    regionCoords[0] = parseInt(regionCoords[0])
    regionCoords[1] = parseInt(regionCoords[1])
    const region = {}
    region.meta = []
    for (let i = 0; i < unit; i += 4) if(file.readUintBE(i, 3)) region.meta.push({start: file.readUintBE(i, 3),count: file.readUint8(i+3), index: i/4})
    region.chunkCount = region.meta.length

    region.buffers = []
    for (let i = 0; i < region.chunkCount; i++){
        region.buffers.push(file.slice(region.meta[i].start*unit, (region.meta[i].start+region.meta[i].count)*unit))
    }

    for(let i = 0; i < region.chunkCount; i++){
        region.meta[i].size = region.buffers[i].readUintBE(0, 4)-1
        region.buffers[i] = region.buffers[i].slice(5, 5+region.meta[i].size)
    }

    region.results = []
    for(let i = 0; i < region.chunkCount; i++)
    {
        region.buffers[i] = zlib.inflateSync(region.buffers[i])
        nbt.parse(region.buffers[i], function(err, data){if (err) throw err; region.buffers[i]=data})
        var coords = []
        coords[2] = region.meta[i].index%32
        coords[3] = region.meta[i].index>>5
        coords[0] = regionCoords[0]*2+coords[2]>>4
        coords[1] = regionCoords[1]*2+coords[3]>>4
        coords[2] %= 16
        coords[3] %= 16
        region.results.push({coords: coords, data: region.buffers[i]})
    }
    callback(region.results)
}