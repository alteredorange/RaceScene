using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Used by components to determine when updates should occur
    /// </summary>
    public enum CurvyUpdateMethod
    {
        Update,
        LateUpdate,
        FixedUpdate
    }

    /// <summary>
    /// Class defining a relative Vector (Position+Direction) on a Curvy Spline or Network
    /// </summary>
    [System.Serializable]
    public class CurvyVectorRelative
    {
        [SerializeField]
        internal float m_Position;
        public float Position
        {
            get { return m_Position; }
            set
            {
                if (m_Position != value)
                {
                    m_Position = value;
                    Validate();
                }
            }
        }

        [SerializeField]
        internal int m_Direction = 1;
        public int Direction
        {
            get
            {
                return m_Direction;
            }
            set
            {
                if (value != m_Direction)
                    if (value > 0)
                        m_Direction = 1;
                    else
                        m_Direction = -1;
            }
        }

        public CurvyVectorRelative()
        {
        }

        public CurvyVectorRelative(CurvyVectorRelative org)
        {
            this.m_Position = org.m_Position;
            this.m_Direction = org.m_Direction;
        }

        public CurvyVectorRelative(float position, int direction)
        {
            Direction = direction;
            Position = position;
        }

        public virtual void Validate()
        {
            m_Position = Mathf.Clamp01(m_Position);
        }
    }

    /// <summary>
    /// Class defining a relative or absolute Vector (Position+Direction) on a Curvy Spline
    /// </summary>
    [System.Serializable]
    public class CurvyVector : CurvyVectorRelative
    {

        public float MaxDistance;

        public CurvyVector() 
        {
            MaxDistance = -1;
        }

        public CurvyVector(CurvyVector org)
        {
            this.MaxDistance = org.MaxDistance;
            this.m_Position = org.m_Position;
            this.m_Direction = org.m_Direction;
        }
        public CurvyVector(float position, int direction) : this(position, direction, -1) { }
        public CurvyVector(float position, int direction, float maxDistance)
        {
            MaxDistance = maxDistance;
            Direction = direction;
            Position = position;
        }

        public void Absolute(float maxDistance)
        {
            MaxDistance = maxDistance;
            Validate();
        }

        public void Relative()
        {
            MaxDistance = -1;
            Validate();
        }

        public void Validate(float maxDistance)
        {
            MaxDistance=maxDistance;
            Validate();
        }

        public override void Validate()
        {
            if (MaxDistance == -1)
                base.Validate();
            else
                m_Position = Mathf.Clamp(m_Position, 0, MaxDistance);
        }
    }

    /// <summary>
    /// Class defining a path range
    /// </summary>
    [System.Serializable]
    public class CurvyRange
    {
        public enum RangeMode
        {
            Relative,
            Absolute,
            NonLinear
        }

        public RangeMode Mode;
        public float From;
        public float To;
        public float MaxDistance;

        public float Length
        {
            get { return To - From; }
        }

        public CurvyRange()
        {
            Mode = RangeMode.Relative;
            From = 0;
            To = 1;
        }

        public CurvyRange(RangeMode mode, float from, float to)
        {
            Mode = mode;
            From = from;
            To = to;
        }

        public static CurvyRange Relative(float from, float to)
        {
            var R = new CurvyRange(RangeMode.Relative, from, to);
            R.Validate();
            return R;
        }

        public static CurvyRange Absolute(float from, float to)
        {
            var R = new CurvyRange(RangeMode.Absolute, from, to);
            R.Validate();
            return R;
        }

        public static CurvyRange NonLinear(float from, float to)
        {
            var R = new CurvyRange(RangeMode.NonLinear, from, to);
            R.Validate();
            return R;
        }

        public void Validate(float maxDistance)
        {
            MaxDistance = maxDistance;
            Validate();
        }

        public void Validate()
        {
            if (From < 0)
                From = 0;
            if (Mode == RangeMode.Absolute)
            {
                if (To > MaxDistance)
                    To = MaxDistance;
            }
            else
            {
                if (From > 1)
                    From = 1;

                if (To > 1)
                    To = 1;
            }

            if (To < From)
                To = From;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            CurvyRange R = obj as CurvyRange;
            if (R == null)
                return false;

            return Equals(R);
        }

        public bool Equals(CurvyRange with)
        {
            return (Mode == with.Mode &&
                    Mathf.Approximately(From, with.From) &&
                    Mathf.Approximately(To, with.To));
        }

        public override int GetHashCode()
        {
            return new { A = Mode, B = From, C = To }.GetHashCode();
        }


    }

    /// <summary>
    /// Class defining the way of sample point creation
    /// </summary>
    [System.Serializable]
    public class CurvyDistribution
    {
        /// <summary>
        /// SamplePoint creation mode
        /// </summary>
        public enum DistributionMode
        {
            /// <summary>
            /// By a fixed TF stepsize
            /// </summary>
            NonLinear,
            /// <summary>
            /// By a fixed distance (in world units)
            /// </summary>
            Distance,
            /// <summary>
            /// By angle of curvation
            /// </summary>
            Adaptive
        }
        
        public DistributionMode Mode;
        /// <summary>
        /// Stepsize in TF
        /// </summary>
        public float Step;
        /// <summary>
        /// Minimum step distance in world units
        /// </summary>
        public float MinDistance;
        /// <summary>
        /// Angle in degrees (used by Adaptive Distribution only)
        /// </summary>
        public float Angle;

        public CurvyDistribution()
        {
            Mode = DistributionMode.Distance;
            Step = 1f;
            MinDistance = 0.2f;
            Angle = 5;
        }

        public CurvyDistribution(DistributionMode mode, float step, float angle, float minDistance)
        {
            Mode = mode;
            Step = step;
            MinDistance = minDistance;
            Angle = angle;
        }

        public static CurvyDistribution NonLinear(float step)
        {
            return new CurvyDistribution(DistributionMode.NonLinear, step, 0, 5);
        }

        public static CurvyDistribution Distance(float step)
        {
            return new CurvyDistribution(DistributionMode.Distance, step, 0, 5);
        }

        public static CurvyDistribution Adaptive(float angle, float minDistance)
        {
            return new CurvyDistribution(DistributionMode.Adaptive, -1, angle, minDistance);
        }

        public void Validate()
        {
            if (Mathf.Approximately(0, Angle))
                Angle = 0.001f;
            if (MinDistance < 0.001f)
                MinDistance = 0.001f;
            if (Step < 0.0001f)
                Step = 0.0001f;
        }

        public bool Equals(CurvyDistribution with)
        {
            return (Mode == with.Mode &&
                    Mathf.Approximately(Step, with.Step) &&
                    Mathf.Approximately(MinDistance, with.MinDistance) &&
                    Mathf.Approximately(Angle, with.Angle));
        }

        public override int GetHashCode()
        {
            return new { A = Mode, B = Step, C = MinDistance, D = Angle }.GetHashCode();
        }



    }

  
}