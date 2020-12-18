using System;
using MathNet.Filtering;
using MathNet.Filtering.Kalman;
using System.Collections.Generic;

namespace NIRSrecorder
{

	public struct MBLLmapping {
		public int[][] measurementPairs;
		public int[][] hboIndex;
		public double[][] ExtCoefHbO;
		public double[][] ExtCoefHbR;
		public double[] distances;
	}

	public partial class RealtimeEngine
	{
        public OnlineFilter[][] OnlineFIRFiltersHPF;
		public OnlineFilter[][] OnlineFIRFiltersHPF2;
		public OnlineFilter[][] OnlineFIRFiltersLPF;
		readonly DiscreteKalmanFilter[][] MocoKalman;
        public int[] nsamples;
		public MBLLmapping[] mBLLmappings;


		public RealtimeEngine()
		{

			nsamples = new int[MainClass.devices.Length];

			bool usehpf = MainClass.win._handles.useHPF.Active;
			bool uselpf = MainClass.win._handles.useLPF.Active;
	
			double hpf = Convert.ToDouble(MainClass.win._handles.editHPF.Text);
			double lpf = Convert.ToDouble(MainClass.win._handles.editLPF.Text);

			double[] fs = new double[MainClass.devices.Length];
			OnlineFIRFiltersHPF = new OnlineFilter[MainClass.devices.Length][];
			OnlineFIRFiltersHPF2 = new OnlineFilter[MainClass.devices.Length][];
			OnlineFIRFiltersLPF = new OnlineFilter[MainClass.devices.Length][];
			MocoKalman = new DiscreteKalmanFilter[MainClass.devices.Length][];

			mBLLmappings = new MBLLmapping[MainClass.devices.Length];

			for (int i = 0; i < MainClass.devices.Length; i++)
			{

				int nSDpairs = MainClass.win.nirsdata[i].probe.numChannels / MainClass.win.nirsdata[i].probe.numWavelengths;

				mBLLmappings[i] = new MBLLmapping();
				mBLLmappings[i].distances = new double[nSDpairs];
				mBLLmappings[i].hboIndex = new int[nSDpairs][];
				mBLLmappings[i].measurementPairs = new int[nSDpairs][];
			
				mBLLmappings[i].ExtCoefHbO = new double[nSDpairs][];
				mBLLmappings[i].ExtCoefHbR = new double[nSDpairs][];

				int cnt = 0;
				int[] cnt2 = new int[nSDpairs];
                for(int ii=0; ii<nSDpairs; ii++) {
                    cnt2[ii] = 0;
					mBLLmappings[i].hboIndex[ii] = new int[2];
					mBLLmappings[i].measurementPairs[ii] = new int[MainClass.win.nirsdata[i].probe.numWavelengths];
					mBLLmappings[i].ExtCoefHbO[ii] = new double[MainClass.win.nirsdata[i].probe.numWavelengths];
					mBLLmappings[i].ExtCoefHbR[ii] = new double[MainClass.win.nirsdata[i].probe.numWavelengths];
				}

				nirs.Media media = new nirs.Media();
				for (int sI=0;sI< MainClass.win.nirsdata[i].probe.numSrc; sI++)
                {
					for (int dI = 0; dI < MainClass.win.nirsdata[i].probe.numDet; dI++)
					{
						bool found = false; ;
						for (int mI = 0; mI < MainClass.win.nirsdata[i].probe.numChannels; mI++)
						{
							if (MainClass.win.nirsdata[i].probe.ChannelMap[mI].sourceindex == sI &
								MainClass.win.nirsdata[i].probe.ChannelMap[mI].detectorindex == dI)
							{
								mBLLmappings[i].measurementPairs[cnt][cnt2[cnt]] = mI + MainClass.win.nirsdata[i].probe.numChannels;
								if (mBLLmappings[i].hboIndex[cnt][0] == 0)
								{

									mBLLmappings[i].hboIndex[cnt][0] = mI + MainClass.win.nirsdata[i].probe.numChannels * 2;
									mBLLmappings[i].hboIndex[cnt][1] = mI + nSDpairs + MainClass.win.nirsdata[i].probe.numChannels * 2;
								}
								
								mBLLmappings[i].distances[cnt] = 0;
								media.GetSpectrum(MainClass.win.nirsdata[i].probe.ChannelMap[mI].wavelength,
									out mBLLmappings[i].ExtCoefHbO[cnt][cnt2[cnt]], out mBLLmappings[i].ExtCoefHbR[cnt][cnt2[cnt]]);

								mBLLmappings[i].distances[cnt] = Math.Sqrt((MainClass.win.nirsdata[i].probe.SrcPos[sI, 0] -
																			MainClass.win.nirsdata[i].probe.DetPos[dI, 0]) *
																			(MainClass.win.nirsdata[i].probe.SrcPos[sI, 0] -
																			MainClass.win.nirsdata[i].probe.DetPos[dI, 0]) +
																			(MainClass.win.nirsdata[i].probe.SrcPos[sI, 1] -
																			MainClass.win.nirsdata[i].probe.DetPos[dI, 1]) *
																			(MainClass.win.nirsdata[i].probe.SrcPos[sI, 1] -
																			MainClass.win.nirsdata[i].probe.DetPos[dI, 1]));
								cnt2[cnt]++;
								found = true;
							}


						}
                        if (found)
                        {
							cnt++;
                        }
					}
				}



				nsamples[i] = 0;
				NIRSDAQ.info info = MainClass.devices[i].GetInfo();
				fs[i] = info.sample_rate;
				OnlineFIRFiltersHPF[i] = new OnlineFilter[MainClass.win.nirsdata[i].probe.numChannels];
				OnlineFIRFiltersLPF[i] = new OnlineFilter[MainClass.win.nirsdata[i].probe.numChannels];
				OnlineFIRFiltersHPF2[i] = new OnlineFilter[MainClass.win.nirsdata[i].probe.numChannels];
				MocoKalman[i] = new DiscreteKalmanFilter[MainClass.win.nirsdata[i].probe.numChannels];

				for (int j = 0; j < MainClass.win.nirsdata[i].probe.numChannels; j++)
				{
					OnlineFIRFiltersLPF[i][j] = OnlineFilter.CreateLowpass(ImpulseResponse.Finite, fs[i], lpf);
					OnlineFIRFiltersHPF[i][j] = OnlineFilter.CreateHighpass(ImpulseResponse.Finite, fs[i], hpf);
					OnlineFIRFiltersHPF2[i][j] = OnlineFilter.CreateHighpass(ImpulseResponse.Finite, fs[i], 0.001);
					
					OnlineFIRFiltersHPF[i][j].Reset();
					OnlineFIRFiltersHPF2[i][j].Reset();
					OnlineFIRFiltersLPF[i][j].Reset();

					int order = 5;
					var x0 = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(order + 1, 1, 0);
					var P0 = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.DenseIdentity(order + 1);

					MocoKalman[i][j] = new DiscreteKalmanFilter(x0, P0);


				}
			}

		}

        public List<nirs.core.Data> UpdateRTengine(List<nirs.core.Data> nirsdata)
		{
			bool useMOCO = MainClass.win._handles.useMOCO.Active;
			bool uselpf = MainClass.win._handles.useLPF.Active;
			bool usehpf = MainClass.win._handles.useHPF.Active;
			int nsamplesNew;

			try
			{
				for (int i = 0; i < nirsdata.Count; i++)
				{
					nsamplesNew = nirsdata[i].data[0].Count;

					for (int tpt = nsamples[i]; tpt < nsamplesNew; tpt++)
					{
						if (MainClass.win._handles.SaveTempFile.Active)
						{
							MainClass.win.TempStreamWriter.Write(String.Format("{0},", nirsdata[i].time[tpt]));
						}
						int nch = nirsdata[i].probe.numChannels;
						for (int j = 0; j < nch; j++)
						{
							if (MainClass.win._handles.SaveTempFile.Active)
							{
								MainClass.win.TempStreamWriter.Write(String.Format("{0},", nirsdata[i].data[j][tpt]));
							}

							// optical density
							double d = -Math.Log(nirsdata[i].data[j][tpt]) + Math.Log(nirsdata[i].data[j][0]);
							d = OnlineFIRFiltersHPF2[i][j].ProcessSample(d);

							if (useMOCO)
							{
								int order = 5;
								if (tpt > order)
								{
									var F = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.DenseIdentity(order + 1);
									var Q = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(order + 1, order + 1, 0);
									var H = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(1, order + 1);
									var R = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(1, 1, .001);
									var z = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(1, 1, d);

									for (int k = 0; k < order; k++)
									{
										H[0, k] = -Math.Log(nirsdata[i].data[j][tpt - k - 1]) + Math.Log(nirsdata[i].data[j][0]);
										//data [1].data [j, i - k - 1];
									}

									MocoKalman[i][j].Predict(F, Q);
									MocoKalman[i][j].Update(z, H, R);

									var ressid = H * MocoKalman[i][j].State;
									d = ressid[0, 0];
								}

							}
							if (uselpf)
							{
								d = OnlineFIRFiltersLPF[i][j].ProcessSample(d);
							}
							else
							{
								_ = OnlineFIRFiltersLPF[i][j].ProcessSample(d);
							}

							if (usehpf)
							{
								d = OnlineFIRFiltersHPF[i][j].ProcessSample(d);
							}
							else
							{
								_ = OnlineFIRFiltersHPF[i][j].ProcessSample(d);
							}

							nirsdata[i].data[j + nch].Add(d);

						}
						if (MainClass.win._handles.SaveTempFile.Active)
						{
							MainClass.win.TempStreamWriter.Write("\r\n");
						}
					}
					// MBLL
					for (int tpt = nsamples[i]; tpt < nsamplesNew; tpt++)
					{
						for (int ch = 0; ch < mBLLmappings[i].distances.Length; ch++)
						{

							double E11 = mBLLmappings[i].ExtCoefHbO[ch][0];
							double E12 = mBLLmappings[i].ExtCoefHbR[ch][0];
							double E21 = mBLLmappings[i].ExtCoefHbO[ch][1];
							double E22 = mBLLmappings[i].ExtCoefHbR[ch][1];
							double L = mBLLmappings[i].distances[ch];

							double d1 = nirsdata[i].data[mBLLmappings[i].measurementPairs[ch][0]][tpt];
							double d2 = nirsdata[i].data[mBLLmappings[i].measurementPairs[ch][1]][tpt];

							double HbO = 1000000 * 1 / L * (E22 * d1 - E12 * d2) / (E11 * E11 - E12 * E21);
							double HbR = 1000000 * 1 / L * (E21 * d2 - E11 * d1) / (E11 * E11 - E12 * E21);


							nirsdata[i].data[mBLLmappings[i].hboIndex[ch][0]].Add(HbO);
							nirsdata[i].data[mBLLmappings[i].hboIndex[ch][1]].Add(HbR);

						}
					}


					nsamples[i] = nsamplesNew;
				}
            }
            catch
            {
				
            }

            return nirsdata;
        }


        /*
			int lastID = 1;
			if (this.useMBLL())
			{
				// MBLL
		
				double E11, E12, E21, E22;
				int sIdx1, sIdx2;

				int numml = data[0].SDGRef.nummeas;
				for (int i = oldsamples + 1; i < samples; i++)
				{
					double[] d = new double[numml * 2];
					double t = data[0].time[i];
					for (int j = 0; j < numml; j++)
					{
						sIdx1 = data[0].SDGRef.measlist[j, 3];
						E11 = AppWindow.SystemController.systemInfo.laserarray[sIdx1].ExtHbO;
						E12 = AppWindow.SystemController.systemInfo.laserarray[sIdx1].ExtHbR;
						sIdx2 = data[0].SDGRef.measlist[j + numml, 3];
						E21 = AppWindow.SystemController.systemInfo.laserarray[sIdx2].ExtHbO;
						E22 = AppWindow.SystemController.systemInfo.laserarray[sIdx2].ExtHbR;

						double L = data[0].SDGRef.distances[j];
						d[j] = 1000000 * 1 / L * (E22 * data[1].data[j, i] - E12 * data[1].data[j + numml, i]) / (E11 * E11 - E12 * E21);
						d[j + numml] = 1000000 * 1 / L * (E21 * data[1].data[j + numml, i] - E11 * data[1].data[j, i]) / (E11 * E11 - E12 * E21);

					}
					data[2].adddata(d, t);
				}
				data[2].SDGRef = data[0].SDGRef;
				data[2].StimInfo = data[0].StimInfo;
				lastID = 2;
			}

			if (useGLM())
			{
				for (int i = oldsamples + 1; i < samples; i++)
				{
					double q = hscaleKalmanQ.Value;
					double[] d = new double[nm];
					double[] t = new double[1];
					t[0] = data[0].time[i];

					var x = data[0].StimInfo.GetDesignMtx(t);

					int order = 5;
					var H = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(1, order + 1);
					var F = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.DenseIdentity(order + 1);
					var Q = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(order + 1, order + 1, q);

					for (int j = 0; j < x.ColumnCount - 1; j++)
					{  //DC regressor is always end
						H[0, j] = x[0, j];
					}
					for (int j = x.ColumnCount - 1; j < order + 1; j++)
					{
						H[0, j] = 0;
					}
					H[0, order] = 1; // DC regressor at the end

					var B = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(order + 1, nm);
					MathNet.Numerics.LinearAlgebra.Matrix<double>[] C = new MathNet.Numerics.LinearAlgebra.Matrix<double>[nm];
					var R = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(1, 1, .001);
					for (int j = 0; j < nm; j++)
					{
						d[j] = data[lastID].data[j, i];

						var z = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(1, 1, d[j]);

						GLMKalman[j].Predict(F, Q);
						GLMKalman[j].Update(z, H, R);

						var b = GLMKalman[j].State;
						for (int k = 0; k < order + 1; k++)
						{
							B[k, j] = b[k, 0];
						}
						C[j] = GLMKalman[j].Cov;

					}
					statswindow.update(B, C, data[2].StimInfo.conditions, samples);

				}
			}


			if (this.checkbuttonDataXfer.Active)
			{
				string dataout = "";
				if (this.checkbuttonDataXfer.Active)
				{
					for (int i = oldsamples + 1; i < samples; i++)
					{
						dataout += String.Format("{0}", data[this.comboboxwhichXfer.Active].time[i]);
						for (int j = 0; j < nm; j++)
						{
							dataout += String.Format("\t{0}", data[this.comboboxwhichXfer.Active].data[j, i]);
						}
						dataout += "\r\n";
					}
				}

				// Transfer the data in real-time to a file
				string file = entryRTfileName.Text;
				string rtXferfile = System.IO.Path.Combine(PhotonViewer.AppWindow.SystemController.subjectData.outfolder, file);
				System.IO.File.AppendAllText(rtXferfile, dataout);
			}


			return data;

		}
        */
	}
}

	 