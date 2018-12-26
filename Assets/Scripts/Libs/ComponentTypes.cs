using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using System.Collections.Generic;
using System;

namespace Master
{
    /// <summary>
    /// Data Components
    /// </summary>

    public struct CurveInfo : IComponentData
    {
        public CurveInfo(int pathID, byte type) : this() { this.pathID = pathID; this.type = type; }
        public byte type;
        public int pathID;
    }

    public struct LastWeight : IComponentData
    {
        public LastWeight(float4 weight) : this() { this.Value = weight; }
        public float4 Value;
    }

    public struct ControlPointID : IComponentData
    {
        public ControlPointID(int ID) : this() { this.Value = ID; }
        public int Value;
    }

    public struct PathID : IComponentData
    {
        public PathID(int ID) : this() { this.Value = ID; }
        public int Value;
    }

    public struct Traveler : IComponentData
    {
        public Traveler(Entity traveler) : this() { this.Value = traveler; }
        public Entity Value;
    }

    public struct UpdateIndicator : IComponentData
    {
        public UpdateIndicator(byte isChanged) : this() { this.Value = isChanged; }
        public byte Value;
    }

    public struct PointPosition : IComponentData
    {
        public PointPosition(float3 position) : this() { this.Value = position; }
        public float3 Value;
    }

    public struct VectorPoints : IComponentData
    {
        public VectorPoints(Entity frontVec, Entity backVec) : this() { this.frontVec = frontVec; this.backVec = backVec; }
        public Entity frontVec;
        public Entity backVec;
    }




    /// <summary>
    /// Buffers
    /// </summary>
    /// 
    public struct TimeListElement : IBufferElementData
    {
        public TimeListElement(float Value) : this() { this.Value = Value; }
        // These implicit conversions are optional, but can help reduce typing.
        public static implicit operator float(TimeListElement e) { return e.Value; }
        public static implicit operator TimeListElement(float e) { return new TimeListElement { Value = e }; }
        // Actual value each buffer element will store.
        public float Value;
    }

    public struct CurveElement : IBufferElementData
    {
        public CurveElement(Entity Value) : this() { this.Value = Value; }
        public static implicit operator Entity(CurveElement e) { return e.Value; }
        public static implicit operator CurveElement(Entity e) { return new CurveElement { Value = e }; }
        // Actual value each buffer element will store.
        public Entity Value;
    }

    public struct ControlPointElement : IBufferElementData
    {
        public ControlPointElement(Entity Value) : this() { this.Value = Value; }
        public static implicit operator Entity(ControlPointElement e) { return e.Value; }

        // Actual value each 
        public Entity Value { get; }
    }

    public struct CPTransform : IBufferElementData
    {
        public CPTransform(Position Value) : this() { this.Value = Value; }
        public static implicit operator Position(CPTransform e) { return e.Value; }

        // Actual value each 
        public Position Value { get; }
    }



    /// <summary>
    /// Markers
    /// </summary>
    // public struct PathMarker : ISharedComponentData { }
    // public struct CurveMarker : ISharedComponentData { }
    // public struct ControlPointMarker : ISharedComponentData { }
    // public struct VectorPointMarker : ISharedComponentData { }
    // public struct TravelerMarker : ISharedComponentData { }

    /// <summary>
    /// Eunums
    /// </summary>
    public struct CurveTypes
    {
        public const byte p5 = 0; // For 5 Points Curve
        public const byte p3v2 = 1; // For 3 Points Curve with Vectors in the Ends
    }

    /// <summary>
    /// Data Collections
    /// </summary>
    
    public struct DualQuaternion
    {
        public DualQuaternion(float4 Wt, float4 Ft) : this()
        { this.Wt = Wt; this.Ft = Ft; }
        public float4 Wt;
        public float4 Ft;

    }

    public struct DisplacementData
    {
        public DisplacementData(float3 position, quaternion rotation) : this()
        { this.position = position; this.rotation = rotation; }
        public float3 position;
        public quaternion rotation;
    }

    public struct FittedMovement
    {
        public FittedMovement(float3[] positions, float4[] rotations) : this()
        { this.positions = positions; this.rotations = rotations; }
        public float3[] positions;
        public float4[] rotations;
    }

    public struct EulerRodriguesFrame
    {
        public EulerRodriguesFrame(float3 vecX, float3 vecY, float3 vecZ) : this()
        {
            this.vecX = vecX; this.vecY = vecY; this.vecZ = vecZ;
        }

        public float3 vecX, vecY, vecZ;
    }


}



