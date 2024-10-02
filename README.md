# Conway's Game of Life - Massive Simulation in Unity

This is a high-performance massive simulation of Conway's Game of Life built using Unity Engine. The main objective was to create a version capable of handling millions of cells at a good frame rate and supporting dynamic configuration changes to the simulation. To achieve this, I utilized Unity's Jobs and Burst frameworks for parallel computation and `Graphics.RenderMeshIndirect` API for efficient rendering.

### Hardware Specs

- Type: Laptop
- CPU: AMD Ryzen™ 9 5900HX with Radeon Graphics
- Cores: 8
- L1 Cache: 512 KB
- L2 Cache: 4 MB
- L3 Cache: 16 MB
- Dedicated GPU: Nvidia GeForce RTX 3060 Laptop GPU

Results with Dedicated GPU :
Passing data from CPU to GPU is the main bottleneck here. This can be avoided by using Compute Shaders! Gonna do this one in the future! Here's the performance breakdown :
- Without rendering, the simulation handles up to 67 million cells at 30 FPS.
- When rendering with quads, it handles up to 33 million cells at 30 FPS.
- When rendering with cubes, it handles up to 4 million cells at 30 FPS.

Results with Integrated GPU :
The simulation is GPU-bottlenecked during rendering since it's NOT using the dedicated GPU. Here's the performance breakdown :
- Without rendering, the simulation handles up to 67 million cells at 30 FPS.
- When rendering with quads, it handles up to 16 million cells at 30 FPS.
- When rendering with cubes, it handles up to 2 million cells at 30 FPS.

## Preview

Build Link: https://drive.google.com/file/d/12VT9OIP2UZEpqWAuwq5kxACuaCH6nrjQ/view?usp=drive_link

Video Montage : [Youtube](https://www.youtube.com/watch?v=uacI5GSx63Y)

![Screenshot_2](https://github.com/user-attachments/assets/d42cbc84-d8eb-473e-a7b9-fc651652a97e)

16 Million at 60 FPS : 
![Screenshot_1](https://github.com/user-attachments/assets/2b94d708-d64d-4b2a-be16-a70b6b9680fe)

3D Grid :
![Screenshot_3](https://github.com/user-attachments/assets/f1c532e0-6b9e-4f91-b168-c22530cd9e4f)

Inside :
![Screenshot_4](https://github.com/user-attachments/assets/b56b3f1d-70bb-47e0-b051-6c03ca4f5c25)
