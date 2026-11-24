
using GlobalEventSystem;
using UnityEngine;

public class LevelInitialized : AEvent<Level> {}
public class GridFallCompleted : AEvent<GameGrid> {}
public class GridObjectDestroyed : AEvent<GridObjectId, Vector3> {}
public class GridObjectClicked : AEvent {}
public class LevelStateChanged : AEvent<Level> {}
