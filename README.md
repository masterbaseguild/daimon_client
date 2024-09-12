# Setup

1. Clone this repository
2. Install Unity from https://unity.com
3. Download Unity version 2022.3.6f1
4. Add the project to Unity Hub and open it

# Prototypes

This project also includes two old prototypes:

- Voxel Engine Prototype (Unity version 2021.3.20f1)
- Modify and Chunk Load Prototype (Unity version 2021.3.20f1)

# Data Structures

## Block

- id

        the id of the block (e.g. "000000000001")
- display

        the display name of the block (e.g. "grass")
- isOpaque

        whether the block is opaque (e.g. true)
- isConcrete

        whether the block is concrete (e.g. true)
- texture

        the texture of the block (e.g. "https://example.com/texture.png")

- texture2D
    
        the texture of the block as a 2D texture (e.g. Texture2D)

- OnTextureLoaded

        the function to call when the texture is loaded (e.g. function)

## User

- username

        the username of the user (e.g. "user")

- index

        the index of the user in the server (e.g. 5)

## Packet

- type

        the type of the packet (e.g. "connect")

- data

        the data of the packet (e.g. ["username"])

# Components

- API Client
- UDP Client
- (Incomplete) Terrain Rendering
- Multithreading

# Todo

- [ ] integrate prototypes into the main project

# Prototype Goals

## Voxel Engine

- Regions made of 16x16x16 chunks, made of 16x16x16 blocks
- Client fetches the region from the server, then fetches the data for all the blocks referenced in the region header
- Client renders the region using the blocks data

### What we are NOT developing right now
- Chunk Culling
- Multithreaded Chunk Loading
- Dynamic Chunk Loading (only load the x nearest chunks)
- Level of Detail (LOD)
- Liquids
- Lighting
- World modification

## Movement

- First person camera, Mouse to look around
- WASD movement
- Space to jump (bunny hopping by holding space), Shift to sprint, Ctrl to crouch
- Gravity and collision detection

### What we are NOT developing right now

- Multiple Cameras
- Advanced Movement (Double Jump, Bullet Jump)

## Multiplayer

- Always Online (connect to an existing server to play)
- UDP Client to send and receive packets
- Custom Packet Protocol
- Chat
- Live sync of other players' positions

### What we are NOT developing right now

- Animation Sync

## Assets

- Block Textures
- Player Model
- Skybox

# Alpha Goals

- Combat System, Advanced Movement
- Inventory System
- Animation System
- Sound Engine
- User Interface
- Authentication and Authorization
- World modification (sandbox mode for building maps)
- Main Map: MasterBase
- Soundtrack
- Item, weapon and ability assets (sprites, textures, models, animations)