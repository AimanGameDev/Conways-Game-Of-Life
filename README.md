# Conway's Game of Life - High Performance Simulation in Unity

This is a high-performance simulation of Conway's Game of Life built using the Unity engine. The main objective was to create a version capable of handling millions of cells at a good frame rate. To achieve this, I utilized Unity's Jobs and Burst frameworks for parallel computation, as well as the Graphics.RenderMeshIndirect API for efficient rendering.

### Hardware Used

This is my hardware specs :

Type: Laptop
Processor: AMD Ryzen™ 9 5900HX with Radeon Graphics
Cores: 8
L1 Cache: 512 KB
L2 Cache: 4 MB
L3 Cache: 16 MB

The simulation is GPU-bottlenecked during rendering. Here's a performance breakdown :
- Without rendering, the simulation handles up to 67 million cells at 30 FPS.
- When rendering with quads, it handles up to 16 million cells at 30 FPS.
- When rendering with cubes, it handles up to 2 million cells at 30 FPS.

So, my plan of moving the computation to compute shaders won't work.

## Preview

Build Link : https://drive.google.com/file/d/12VT9OIP2UZEpqWAuwq5kxACuaCH6nrjQ/view?usp=drive_link

Video Montage : [Youtube](https://youtu.be/6linvymvMDA)

Default : 
![COG_Default](https://github.com/user-attachments/assets/3bb64a11-cd61-4a45-b95a-356968f720e7)

33 Million Cells : 
![COG_33Million](https://github.com/user-attachments/assets/3587c6b5-baab-4c0c-b14f-093d21b60d6d)

Close up :
![COG_CloseShot](https://github.com/user-attachments/assets/0692fb21-ef4d-4359-924f-743eaef710a5)

3D Grid :
![COG_Cube](https://github.com/user-attachments/assets/390ee1bd-6ebb-4d0d-95bb-1798be47fb02)
