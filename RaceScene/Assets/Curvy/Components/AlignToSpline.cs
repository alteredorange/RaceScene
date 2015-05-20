using UnityEngine;
using System.Collections;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Controller to align an object to a spline
    /// </summary>
    public class AlignToSpline : CurvyComponent
    {
        /// <summary>
        /// Whether to use relative or absolute positions
        /// </summary>
        public enum AlignMode
        {
            /// <summary>
            /// Use relative position (0..1)
            /// </summary>
            Relative,
            /// <summary>
            /// Use absolute position (World Units)
            /// </summary>
            Absolute
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
                    if (m_Spline)
                        m_Spline.OnRefresh -= OnSplineRefresh;

                    m_Spline = value;
                    doUpdate();
                }
            }
        }
        [SerializeField]
        AlignMode m_Mode = AlignMode.Relative;
        /// <summary>
        /// The Alignment mode
        /// </summary>
        public AlignMode Mode
        {
            get { return m_Mode; }
            set
            {
                if (m_Mode != value)
                {
                    m_Mode = value;
                    doUpdate();
                }
            }
        }


        /// <summary>
        /// Whether cached values should be interpolated if applicable
        /// </summary>
        [Label(Tooltip = "Interpolate cached values?")]
        public bool FastInterpolation;

        [SerializeField]
        float m_Position;
        /// <summary>
        /// Gets or sets the position (either TF or world units, depending on AlignMode)
        /// </summary>
        public float Position
        {
            get { return m_Position; }
            set 
            {
                if (m_Position != value)
                {
                    m_Position = value;
                    doUpdate();
                }
            }
        }

        /// <summary>
        /// Whether object should be aligned to Up-Vector or not
        /// </summary>
        [Label(Tooltip = "Align to Up-Vector?")]
        public bool SetOrientation = true;

        /// <summary>
        /// Whether rotation should be locked to z-Axis
        /// </summary>
        [Label(Text = "Use 2D Orientation", Tooltip = "Use 2D Orientation (along z-axis only)?")]
        public bool Use2DOrientation = false;

        [SerializeField]
        [Label(Tooltip = "Enable to recalculate position when spline segments are added/removed during runtime")]
        bool m_Dynamic = true;
        /// <summary>
        /// If checked position will be recalculated when segments are added or removed from the spline
        /// </summary>
        public bool Dynamic
        {
            get { return m_Dynamic; }
            set
            {
                if (m_Dynamic != value)
                {
                    m_Dynamic = value;
                    doUpdate();
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
                if (Dynamic)
                    return mCurrentSegment;
                else
                    return Spline.TFToSegment(mCurrentTF, out mCurrentSegmentF);
            }
        }

        /// <summary>
        /// Gets the current segment's localF
        /// </summary>
        public float CurrentSegmentF { get { return mCurrentSegmentF; } }
        
        /// <summary>
        /// Gets the current TF
        /// </summary>
        /// <remarks>Use Position to alter the current position</remarks>
        public float CurrentTF { get { return mCurrentTF; } }

        /// <summary>
        /// Event raised when the segment we're over turns Null
        /// </summary>
        public CurvyComponentEvent OnNullSegment;

        #endregion

        bool mIsInitialized;
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
        protected float mCurrentSegmentF;
        protected float mCurrentTF;
        bool mNeedRefresh;

        #region ### Unity Callbacks ###

        IEnumerator Start()
        {
            if (Spline)
            {
                while (!SourceIsInitialized)
                    yield return 0;
                doUpdate();
            }
        }

        void OnEnable()
        {
            if (!Application.isPlaying)
                doUpdate();
        }

        void Update()
        {
            if ((UpdateIn == CurvyUpdateMethod.Update && mNeedRefresh) || !Application.isPlaying)
                doUpdate();
        }
        void LateUpdate()
        {
            if (UpdateIn == CurvyUpdateMethod.LateUpdate && mNeedRefresh)
                doUpdate();
        }
        void FixedUpdate()
        {
            if (UpdateIn == CurvyUpdateMethod.FixedUpdate && mNeedRefresh)
                doUpdate();
        }

        void OnValidate()
        {
            doUpdate();
        }

        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Triggers manual update
        /// </summary>
        public void Refresh()
        {
            doUpdate();
        }

        #endregion

        #region ### Virtual Methods & Properties ###

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
        /// Used to validate and set m_Position, mCurrentTF, mCurrentSegment, mCurrentSegmentF
        /// </summary>
        protected virtual void Validate()
        {
            switch (Mode)
            {
                case AlignMode.Relative:
                    m_Position = Mathf.Clamp01(m_Position);
                    mCurrentTF = m_Position;
                    break;
                case AlignMode.Absolute:
                    m_Position = Mathf.Clamp(m_Position, 0, Spline.Length);
                    mCurrentTF = Spline.DistanceToTF(m_Position);
                    break;
            }
            if (Dynamic || (!Dynamic && mCurrentSeg==null) || !Application.isPlaying)
                mCurrentSegment = Spline.TFToSegment(mCurrentTF, out mCurrentSegmentF);
        }

        /// <summary>
        /// Used to set Transform
        /// </summary>
        protected virtual void DoRefresh()
        {
            if (mCurrentSegment == null)
            {
                if (OnNullSegment!=null)
                    OnNullSegment(this);
                gameObject.SetActive(false);
                return;
            }

            if (FastInterpolation)
                Transform.position = mCurrentSegment.InterpolateFast(mCurrentSegmentF);
            else
                Transform.position = mCurrentSegment.Interpolate(mCurrentSegmentF);

            if (SetOrientation)
            {
                if (Use2DOrientation)
                    Transform.rotation = Quaternion.LookRotation(Vector3.forward, mCurrentSegment.GetTangentFast(mCurrentSegmentF));
                else
                    Transform.rotation = mCurrentSegment.GetOrientationFast(mCurrentSegmentF);
            }
                

            mNeedRefresh = false;
        }

        #endregion

        #region ### Privates & Internals ###

        void doUpdate()
        {
            if (!SourceIsInitialized)
                return;

            m_Spline.OnRefresh -= OnSplineRefresh;
            m_Spline.OnRefresh += OnSplineRefresh;

            Validate();
            DoRefresh();
        }

        void OnSplineRefresh(CurvySplineBase sender)
        {
            mNeedRefresh=true;
        }

        #endregion


    }
}