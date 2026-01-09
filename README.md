# Sin3D

Sin3D is a lightweight MonoGame extension library designed to reduce boilerplate and make creating 3D games feel as simple as working in 2D.

It provides clean, high-level abstractions for cameras, rendering, models, and collision detection — without hiding or replacing MonoGame’s core.

---

## Features

### Camera
- Simple, configurable `Camera3D` system

### Renderer
- `Renderer3D` for concise, single-line draw calls

### Model Abstraction
A `Model3D` class that manages:
- Position
- Rotation (quaternion-based)
- Scale
- World matrix
- Model texture
- Collision detection with other models (including multi-mesh models)

### Collision Detection
- Bounding spheres
- Axis-aligned bounding boxes (AABB)
- Oriented bounding boxes (OBB)
- Optimized collision pipeline: Bounding Sphere → AABB → OBB

---

## Lightweight & Flexible
- Minimal overhead
- Designed to extend MonoGame, not replace it

---

## Requirements

Sin3D requires a depth buffer.

Ensure `PreferredDepthStencilFormat` is set **before** creating a `Renderer3D`:

```csharp
_graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
