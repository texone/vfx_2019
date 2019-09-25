﻿/*
 * Copyright (c) 2013 christianr.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the GNU Lesser Public License v3
 * which accompanies this distribution, and is available at
 * http://www.gnu.org/licenses/lgpl-3.0.html
 * 
 * Contributors:
 *     christianr - initial API and implementation
 */
namespace cc.creativecomputing.math.signal
{
	using UnityEngine;
	using System;

	/// <summary>
	/// @author christianriekoff
	/// 
	/// </summary>
	[Serializable]
	public class CCSmoothSquareSignal : CCSignal
	{
		[Range(0, 1)]
		public float ratio = 0.5f;
        [Range(0, 1)]
        public float delta = 0.01f;

		private float TriValue(float theInput)
		{
            float myResult = Mathf.Atan(Mathf.Sin(2 * Mathf.PI * theInput) / delta) / Mathf.PI + 0.5f;// 1 - Mathf.Acos((1 - delta) * Mathf.Sin(2 * Mathf.PI  * theInput)) / Mathf.PI;

            if (!normed)
			{
				myResult = myResult * 2 - 1;
			}
			return myResult;
		}

		public override float[] signalImpl(float theX, float theY, float theZ)
		{
			return new float[]{(TriValue(theX) + TriValue(theY)) / 2};
		}

		/* (non-Javadoc)
		 * @see cc.creativecomputing.math.signal.CCSignal#signalImpl(float, float)
		 */
		public override float[] signalImpl(float theX, float theY)
		{
			return new float[]{(TriValue(theX) + TriValue(theY)) / 2};
		}

		/* (non-Javadoc)
		 * @see cc.creativecomputing.math.signal.CCSignal#signalImpl(float)
		 */
		public override float[] signalImpl(float theX)
		{
			return new float[]{ TriValue(theX)};
		}

	}

}