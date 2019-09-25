using System;

namespace cc.creativecomputing.math.signal
{
	
	[Serializable]
	public class CCSwitchSignal : CCSignal
	{

		public CCSignalType signal = CCSignalType.SIMPLEX;

		public override float[] signalImpl(float theX, float theY, float theZ)
		{
			return signal.signal().signalImpl(theX, theY, theZ);
		}

		public override float[] signalImpl(float theX, float theY)
		{
			return signal.signal().signalImpl(theX, theY);
		}

		public override float[] signalImpl(float theX)
		{
			return signal.signal().signalImpl(theX);
		}

	}

}