** Summary **
- Sin3D is a lightweight, MonoGame Extension Library created with the aim of reducing boilerplate and makke creating 3D games as simple as 2D.
- It provides clean high-level abstractions for cameras, rendering, models and collision detection - without hiding MonoGame's core concepts.


** Features **
- Camera: An easy-to-implement Sin3DCamera system.

- Renderer: An easy-to-implement Sin3DRenderer object that allows for single-line draw statements.

- Model Abstraction: A custom Sin3DModel class that handles:
    - Position
    - Rotation
    - Scale
    - World matrix
    - Texture
    - Collision detection with other models

- Collision Detection:
    - Bounding spheres
    - Axis-aligned bounding boxes (AABB)
    - Oriented bounding boxes (OBB)
    - Optimized collision detection pipeline (bounding sphere -> AABB -> OBB)


** Lightweight & Flexible **
- Minimal overhead
- Designed to extend MonoGame, not replace


Note: Sin3D still requires a depth buffer. Ensure PreferredDepthStencilFormat is set before The creation of a Sin3DRenderer to avoid any errors:
_graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
