﻿//
//   Copyright 2017 Mikhail Vasilyev
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Implementation of Hilbert Transform and spectral analysis
/// </summary>
namespace Processing
{
    public interface ISpectralAnalysis
    {
        Complex[] AnalyticFunction { get; }

        double[] InstAmplitudes { get; }

        double[] InstPhases { get; }

        double[] InstFrequencies { get; }
    }

    public class SpectralAnalyzer : ISpectralAnalysis
    {
        /// <summary>
        /// Performs Hilbert Transform on the given yValues
        /// </summary>
        /// <param name="yValues"></param>
        /// <param name="xValues"></param>
        /// <returns></returns>
        public static ISpectralAnalysis Analyze(double[] yValues, double[] xValues)
        {
            double timestep = 0;
            for (int i = 0; i < xValues.Length - 1; ++i) {
                timestep += xValues[i + 1] - xValues[i];
            }
            timestep /= (xValues.Length - 1);

            return new SpectralAnalyzer(yValues, timestep);
        }
        
        private readonly Complex[] _analyticFunction;
        private readonly double[] _instAmplitudes;
        private readonly double[] _instPhases;
        private readonly double[] _instFrequencies;

        protected SpectralAnalyzer(double[] realYValues, double avgTimestep)
        {
            _analyticFunction = HilbertTransform(realYValues);

            _instAmplitudes = new double[_analyticFunction.Length];
            _instPhases = new double[_analyticFunction.Length];
            for (int i = 0; i < _analyticFunction.Length; ++i) {
                _instAmplitudes[i] = _analyticFunction[i].Magnitude;
                _instPhases[i] = _analyticFunction[i].Phase;
            }

            _instFrequencies = new double[_analyticFunction.Length - 1];
            for (int i = 0; i < _instFrequencies.Length; ++i) {
                _instFrequencies[i] = (_instPhases[i + 1] - _instPhases[i]) / avgTimestep;
            }
        }

        Complex[] ISpectralAnalysis.AnalyticFunction { get => _analyticFunction; }

        double[] ISpectralAnalysis.InstAmplitudes { get => _instAmplitudes; }

        double[] ISpectralAnalysis.InstPhases { get => _instPhases; }

        double[] ISpectralAnalysis.InstFrequencies { get => _instFrequencies; }

        protected virtual Complex[] HilbertTransform(double[] xr)
        {
            // Matlab Hilbert Transform

            Complex[] x = new Complex[xr.Length];
            for (int i = 0; i < xr.Length; ++i) {
                x[i] = xr[i];
            }
            Fourier.BluesteinForward(x, FourierOptions.Matlab);
            double[] h = new double[x.Length];
            h[0] = 1;
            for (int i = 1; i < x.Length / 2; ++i) {
                h[i] = 2;
            }
            for (var i = 0; i < x.Length; i++)
                x[i] *= h[i];
            Fourier.BluesteinInverse(x, FourierOptions.Matlab);
            return x.Take(xr.Length).ToArray();
        }
    }

}
