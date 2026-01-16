# Sin3D

Sin3D is a lightweight MonoGame extension library designed to reduce boilerplate and make creating 3D games feel as simple as working in 2D.

It provides clean, high-level abstractions for cameras, rendering, models, and collision detection — without hiding or replacing MonoGame’s core.

---

## Features

### Camera
`Camera3D` class:
- Handles view and projection matrices.

### Renderer
`Renderer3D` class:
- Toggleable default BasicEffect lighting.
- Configurable directional BasicEffect lighting.
- Configurable fog.
- Single-line render calls.

### Model Abstraction
`Model3D` class:
- Handles position.
- Handles rotation (quaternion-based).
- Handles scale.
- Handles World matrix.
- Texture mapping (including per-mesh texture-mapping for multi-meshed models).
- Collision detection with other models (including multi-mesh models).

### Collision Detection
- Bounding spheres
- Axis-aligned bounding boxes (AABB)
- Oriented bounding boxes (OBB)
- Optimized collision pipeline: Bounding Sphere → AABB → OBB

---

## Requirements

Sin3D requires a depth buffer.

Ensure `PreferredDepthStencilFormat` is set **before** creating a `Renderer3D`:

```csharp
_graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
```

---

## How To Get Started:
- Install MonoGame and add to your project.
- Install Sin3D and add to your project.
- Start coding! (sample projects can be found in the sample projects folder ready for you to build).
