// =====================================================================
// Copyright 2013-2014 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy.Utils;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Controller to move an object along a spline
    /// </summary>
    public class FollowSpline : CurvyComponent
    {
        /// <summary>
        /// Movement method options
        /// </summary>
        public enum FollowMode
        {
            /// <summary>
            /// Move by F
            /// </summary>
            Relative,
            /// <summary>
            /// Move by extrapolated distance
            /// </summary>
            AbsoluteExtrapolate,
            /// <summary>
            /// Move by calculated distance
            /// </summary>
            AbsolutePrecise
        }

        #region ### Public Fields & Properties ###

        [SerializeField]
        [Label(Tooltip = "The Spline or Splinegroup to use")]
        CurvySplineBase m_Spline;
        /// <summary>
        /// The Spline or SplineGroup to use
        /// </summary>
        public CurvySplineBase Spline
        {
            get
            {
                return m_Spline;
            }
            set
            {
                if (m_Spline != value)
                {
                    m_Spline = value;
                    Initialize();
                }
            }
        }
        /// <summary>
        /// Whether cached values should be interpolated if applicable
        /// </summary>
        [Label(Tooltip = "Interpolate cached values?")]
        public bool FastInterpolation;
        
        [SerializeField]
        [Label(Tooltip = "Movement Mode")]
        FollowMode m_Mode= FollowMode.Relative;
        /// <summary>
        /// Determines how movement will be calculated
        /// </summary>
        public FollowMode Mode
        {
            get { return m_Mode; }
            set
            {
                if (m_Mode != value)
                {
                    m_Mode = value;
                    Initialize();
                }
            }
        }

        [SerializeField]
        CurvyVector m_Initial = new CurvyVector();
        /// <summary>
        /// Initial Position, either F or world units (depending on Mode)
        /// </summary>
        public CurvyVector Initial
        {
            get { return m_Initial; }
            set
            {
                if (m_Initial != value)
                {
                    m_Initial = value;
                    if (Mode != FollowMode.Relative && SourceIsInitialized)
                        m_Initial.Validate(Spline.Length);
                    else
                        m_Initial.Validate();
                }
            }
        }

        /// <summary>
        /// the moving speed, either in F or world units (depending on Mode)
        /// </summary>
        [Positive(Tooltip = "Speed in F or World Units (depending on Mode)")]
        public float Speed;

        /// <summary>
        /// Determines End Of Spline Behaviour
        /// </summary>
        [Label(Tooltip="End of Spline Behaviour")]
        public CurvyClamping Clamping = CurvyClamping.Clamp;

        /// <summary>
        /// Whether object should be aligned to Up-Vector or not
        /// </summary>
        [Label(Tooltip = "Align to Up-Vector?")]
        public bool SetOrientation = true;
        /// <summary>
        /// Whether rotation should be locked to z-Axis
        /// </summary>
        [Label(Text="Use 2D Orientation",Tooltip="Use 2D Orientation (along z-axis only)?")]
        public bool Use2DOrientation = false;
        

        [SerializeField]
        [Label(Tooltip = "Enable if you plan to add/remove segments during movement")]
        bool m_Dynamic;
        /// <summary>
        /// If checked it's save to add/remove segments of the source spline
        /// </summary>
        public bool Dynamic
        {
            get { return m_Dynamic; }
            set
            {
                if (m_Dynamic != value)
                {
                    if (SourceIsInitialized)
                        mCurrentSegment = Spline.TFToSegment(mCurrentTF, out mCurrentSegmentF);
                    m_Dynamic = value;
                }
            }
        }

        /// <summary>
        /// Gets the current segment the object is moving on
        /// </summary>
        public CurvySplineSegment CurrentSegment
        {
            get 
            {
                if (m_Dynamic)
                    return mCurrentSegment;
                else
                    return Spline.TFToSegment(mCurrentTF, out mCurrentSegmentF);
            }
        }
        /// <summary>
        /// Gets the current segment's localF
        /// </summary>
        public float CurrentSegmentF {
            get 
            {
                if (mCurrentSegmentF==-1)
                    mCurrentSegment=Spline.TFToSegment(mCurrentTF, out mCurrentSegmentF);
                return mCurrentSegmentF; 
            } 
        }
        /// <summary>
        /// Gets the current TF
        /// </summary>
        /// <remarks>Use Position to alter the current position</remarks>
        public float CurrentTF { get { return mCurrentTF; } }

        #endregion

        
        bool mIsInitialized;
        CurvyVector Current;
        System.WeakReference mCurrentSeg;
        protected CurvySplineSegment mCurrentSegment
        {
            get
            {
                return (mCurrentSeg == null) ? null : (CurvySplineSegment)mCurrentSeg.Target;
            }
            set
            {
                if (mCurrentSeg == null)
                    mCurrentSeg = new System.WeakReference(value);
                else
                    mCurrentSeg.Target = value;
            }
        }
        protected float mCurrentSegmentF = -1;
        protected float mCurrentTF;
        protected delegate void Action();
        protected Action UpdateAction;
        

        #region ### Unity Callbacks ###

        IEnumerator Start()
        {
            if (Spline)
            {
                while (!SourceIsInitialized)
                    yield return 0;
                Initialize();
            }
        }

        void OnEnable()
        {
            if (!Application.isPlaying)
                Initialize();
        }

        void Update()
        {
            if (UpdateIn == CurvyUpdateMethod.Update && Application.isPlaying)
                doUpdate();
        }
        void LateUpdate() 
        {
            if (UpdateIn == CurvyUpdateMethod.LateUpdate)
                doUpdate();
        }
        void FixedUpdate() 
        {
            if (UpdateIn == CurvyUpdateMethod.FixedUpdate)
                doUpdate();
        }

        void OnValidate()
        {
            Initialize();
        }

        #endregion

        #region ### Virtual Methods && Properties ###

        /// <summary>
        /// Gets or sets the current position
        /// </summary>
        public virtual float Position
        {
            get
            {
                switch (Mode)
                {
                    case FollowMode.AbsoluteExtrapolate:
                    case FollowMode.AbsolutePrecise:
                        return Spline.TFToDistance(mCurrentTF);
                    default:
                        return mCurrentTF;
                }
            }
            set
            {
                Current.Position = value;
                switch (Mode)
                {
                    case FollowMode.AbsoluteExtrapolate:
                    case FollowMode.AbsolutePrecise:
                        mCurrentTF = Spline.DistanceToTF(value);
                        break;
                    default:
                        mCurrentTF = value;
                        break;
                }
            }
        }

        public virtual void Refresh()
        {
            UpdateAction();
        }

        #endregion

        #region #### Other Methods ###

        /// <summary>
        /// Used by Editor Preview, don't call directly!
        /// </summary>
        public override void EditorUpdate()
        {
            base.EditorUpdate();
            doUpdate();
        }

        /// <summary>
        /// Gets whether the source is fully initialized and ready to use
        /// </summary>
        protected virtual bool SourceIsInitialized
        {
            get
            {
                return (Spline && Spline.IsInitialized);
            }
        }

        /// <summary>
        /// Initialization of the component
        /// </summary>
        /// <returns>true if the component is fully initialized</returns>
        public override bool Initialize()
        {
            if (!SourceIsInitialized)
                return false;

            switch (Mode)
            {
                case FollowMode.AbsoluteExtrapolate:
                    UpdateAction = UpdateAbsoluteExtrapolate;
                    Initial.Absolute(Spline.Length);
                    Current = new CurvyVector(Initial);
                    mCurrentTF = Spline.DistanceToTF(Current.m_Position);
                    break;
                case FollowMode.AbsolutePrecise:
                    UpdateAction = UpdateAbsolutePrecise;
                    Initial.Absolute(Spline.Length);
                    Current = new CurvyVector(Initial);
                    mCurrentTF = Spline.DistanceToTF(Current.m_Position);
                    break;
                default:
                    UpdateAction = UpdateRelative;
                    Initial.Relative();
                    Current = new CurvyVector(Initial);
                    mCurrentTF = Current.Position;
                    break;
            }
            mCurrentSegment = null;
            mCurrentSegmentF = 0;
            Transform.position = Spline.Interpolate(mCurrentTF);
            if (SetOrientation)
                orientate();
            return true;
        }

        /// <summary>
        /// Update Method used by Relative Mode
        /// </summary>
        protected void UpdateRelative()
        {
            if (FastInterpolation)
            {
                Transform.position = Spline.MoveFast(ref mCurrentTF, ref Current.m_Direction, Speed * CurvyUtility.DeltaTime, Clamping);
            }
            else
            {
                Transform.position = Spline.Move(ref mCurrentTF, ref Current.m_Direction, Speed * CurvyUtility.DeltaTime, Clamping);
            }

            if (SetOrientation)
                orientate();
        }

        /// <summary>
        /// Update Method used by AbsoluteExtrapolate Mode
        /// </summary>
        protected void UpdateAbsoluteExtrapolate()
        {
            if (FastInterpolation)
            {
                Transform.position = Spline.MoveByFast(ref mCurrentTF, ref Current.m_Direction, Speed * CurvyUtility.DeltaTime, Clamping);
            }
            else
            {
                Transform.position = Spline.MoveBy(ref mCurrentTF, ref Current.m_Direction, Speed * CurvyUtility.DeltaTime, Clamping);
            }

            if (SetOrientation)
                orientate();
        }

        /// <summary>
        /// Update Method used by UpdateAbsolutePrecise Mode
        /// </summary>
        protected void UpdateAbsolutePrecise()
        {
            Transform.position = Spline.MoveByLengthFast(ref mCurrentTF, ref Current.m_Direction, Speed * CurvyUtility.DeltaTime, Clamping);
            if (SetOrientation)
                orientate();
        }

        #endregion

        #region ### Privates & Internals ###

        void orientate()
        {
            if (Use2DOrientation)
                Transform.rotation = Quaternion.LookRotation(Vector3.forward, Spline.GetTangentFast(mCurrentTF));
            else
                Transform.rotation = Spline.GetOrientationFast(mCurrentTF);

        }

        void doUpdate()
        {
            if (!SourceIsInitialized)
                return;
            if (!mIsInitialized)
            {
                mIsInitialized = Initialize();
                if (!mIsInitialized)
                    return;
            }

            if (Dynamic)
            {
                if (mCurrentSegment)
                {
                    mCurrentTF = Spline.SegmentToTF(mCurrentSegment, mCurrentSegmentF);
                }
                Refresh();
                mCurrentSegment = Spline.TFToSegment(mCurrentTF, out mCurrentSegmentF);
                
            }
            else
                Refresh();
        }

        #endregion

    }
}