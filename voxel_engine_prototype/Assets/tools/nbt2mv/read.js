var fs = require('fs'),
    nbt = require('nbt');

var data = fs.readFileSync('../chunks/r.-1.-1.mca_0.nbt')
var obj
nbt.parse(data, function(err, data){if (err) throw err; obj=data})
console.log(obj)
obj = JSON.stringify(obj)
fs.writeFile('data.json', obj, (err) => {
    if (err) {
      console.log('Error writing file', err);
    } else {
      console.log('File saved successfully');
    }
  });