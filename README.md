# Sin3D
A light-weight, MonoGame Extension Library created with the aim of reducing boilerplate and making 3D games as easy to create as 2D games

Contains:

camera class - handles view and projection matrices for the user

renderer class - handles the rendering of 3d models and fog, making drawing a 3d model as easy as drawing a 2d primitive

model class - a model class containing its own world matrix and texture, with built in methods for refreshing the world matrix and collision detection

oriented bounding box class - a class represeting the next step after axis-aligned bounding boxes - used for an optimized 3d collision detection pipeline
