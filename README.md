Parser for reading ISO 10303-21 STEP files.  

I needed to extract certain geometric elements from STEP files, so I put together this very basic module to handle the parsing phase of the process.
This can be easily extended to other ISO 10303 formats by swapping the EXPRESS schema

Various express schemas are avaliable here: https://github.com/stepcode/stepcode/tree/develop/data


Sample output of tree print after parsing:


1 APPLICATION_PROTOCOL_DEFINITION
└──2 APPLICATION_CONTEXT
3 SHAPE_DEFINITION_REPRESENTATION
├──4 PRODUCT_DEFINITION_SHAPE
│  └──5 PRODUCT_DEFINITION
│     ├──6 PRODUCT_DEFINITION_FORMATION_WITH_SPECIFIED_SOURCE
│     │  └──7 PRODUCT
│     │     └──8 MECHANICAL_CONTEXT
│     │        └──2 APPLICATION_CONTEXT
│     └──9 DESIGN_CONTEXT
│        └──2 APPLICATION_CONTEXT
└──10 ADVANCED_BREP_SHAPE_REPRESENTATION
   ├──11 AXIS2_PLACEMENT_3D
   │  ├──12 CARTESIAN_POINT
   │  ├──13 DIRECTION
   │  └──14 DIRECTION
   ├──15 MANIFOLD_SOLID_BREP
   │  └──16 CLOSED_SHELL
   │     ├──17 ADVANCED_FACE
   │     │  ├──18 FACE_BOUND
   │     │  │  └──19 EDGE_LOOP
   │     │  │     ├──20 ORIENTED_EDGE
   │     │  │     │  └──21 EDGE_CURVE
   │     │  │     │     ├──22 VERTEX_POINT
   │     │  │     │     │  └──23 CARTESIAN_POINT
   │     │  │     │     ├──24 VERTEX_POINT
   │     │  │     │     │  └──25 CARTESIAN_POINT
   │     │  │     │     └──26 SURFACE_CURVE
   │     │  │     │        ├──27 LINE
   │     │  │     │        │  ├──28 CARTESIAN_POINT
   │     │  │     │        │  └──29 VECTOR
   │     │  │     │        │     └──30 DIRECTION
   │     │  │     │        ├──31 PCURVE
   │     │  │     │        │  ├──32 PLANE
   │     │  │     │        │  │  └──33 AXIS2_PLACEMENT_3D
   │     │  │     │        │  │     ├──34 CARTESIAN_POINT
   │     │  │     │        │  │     ├──35 DIRECTION
   │     │  │     │        │  │     └──36 DIRECTION
   │     │  │     │        │  └──37 DEFINITIONAL_REPRESENTATION
   │     │  │     │        │     ├──38 LINE
   │     │  │     │        │     │  ├──39 CARTESIAN_POINT
   │     │  │     │        │     │  └──40 VECTOR
   │     │  │     │        │     │     └──41 DIRECTION
   │     │  │     │        │     └──42 COMPLEX