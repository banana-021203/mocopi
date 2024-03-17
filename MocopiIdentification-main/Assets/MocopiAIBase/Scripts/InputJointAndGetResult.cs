using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using TMPro;
using System;

using MocopiDistinction;  // MocopiDistinctionAIが含まれるネームスペースを使用
namespace MocopiDistinction
{
    internal class InputJointAndGetResult : MonoBehaviour
    {
        [SerializeField] private MocopiDistinctionAI mocopiDistinctionAI = null;
        private Transform _rootTransform = null;
        private Transform _lUpLegTransform = null;
        private Transform _rUpLegTransform = null;
        private Transform _lLowLegTransform = null;
        private Transform _rLowLegTransform = null;
        private Transform _lFootTransform = null;
        private Transform _rFootTransform = null;
        private Transform _firstTorsoTransform = null;
        private Transform _thirdTorsoTransform = null;
        private Transform _fifthTorsoTransform = null;
        private Transform _seventhTorsoTransform = null;
        private Transform _rShoulderTransform = null;
        private Transform _lShoulderTransform = null;
        private Transform _firstNeckTransform = null;
        private Transform _secondNeckTransform = null;
        private Transform _rUpArmTransform = null;
        private Transform _lUpArmTransform = null;
        private Transform _rLowArmTransform = null;
        private Transform _lLowArmTransform = null;
        private Transform _rHandTransform = null;
        private Transform _lHandTransform = null;
        private const int WINDOW_SIZE = 20;
        private const int STEP_SIZE = 1;
        private List<List<float>> windowsData = new List<List<float>>();
        private float[][] window_jug;
        private List<float[]> windowedData = new List<float[]>(); // To store formatted window data
        float timer = 0.0f;
        float interval = 0.1f;  // Time interval for data collection
        public TMP_Text resultText;
        // Transform members initialization...
        private static List<List<float>> listOfLists = new List<List<float>>();
        void Start()
        {
            // Initialize your transforms here...
            _rootTransform = GameObject.Find("human_low:_root").transform;
            _lUpLegTransform = GameObject.Find("human_low:_l_up_leg").transform;
            _rUpLegTransform = GameObject.Find("human_low:_r_up_leg").transform;
            _lLowLegTransform = GameObject.Find("human_low:_l_low_leg").transform;
            _rLowLegTransform = GameObject.Find("human_low:_r_low_leg").transform;
            _lFootTransform = GameObject.Find("human_low:_l_foot").transform;
            _rFootTransform = GameObject.Find("human_low:_r_foot").transform;
            _firstTorsoTransform = GameObject.Find("human_low:_torso_1").transform;
            _thirdTorsoTransform = GameObject.Find("human_low:_torso_3").transform;
            _fifthTorsoTransform = GameObject.Find("human_low:_torso_5").transform;
            _seventhTorsoTransform = GameObject.Find("human_low:_torso_7").transform;
            _rShoulderTransform = GameObject.Find("human_low:_r_shoulder").transform;
            _lShoulderTransform = GameObject.Find("human_low:_l_shoulder").transform;
            _firstNeckTransform = GameObject.Find("human_low:_neck_1").transform;
            _secondNeckTransform = GameObject.Find("human_low:_neck_2").transform;
            _rUpArmTransform = GameObject.Find("human_low:_r_up_arm").transform;
            _lUpArmTransform = GameObject.Find("human_low:_l_up_arm").transform;
            _rLowArmTransform = GameObject.Find("human_low:_r_low_arm").transform;
            _lLowArmTransform = GameObject.Find("human_low:_l_low_arm").transform;
            _rHandTransform = GameObject.Find("human_low:_r_hand").transform;
            _lHandTransform = GameObject.Find("human_low:_l_hand").transform;
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    timer += Time.deltaTime;
                    if (timer >= interval)
                    {
                        // Collect current motion data
                        List<float> currentData = GetMotionData();
                        windowsData.Add(currentData);
                        if (windowsData.Count >= WINDOW_SIZE)
                        {
                            // Format and store the windowed data
                            // var window = windowsData.Take(WINDOW_SIZE).SelectMany(x => x).ToArray();
                            var window = windowsData.Take(WINDOW_SIZE).ToList();

                                                        // AIの推論を実行し、結果を取得
                            window_jug = ConvertToJaggedArray(window);
                            var motionData = new MocopiDistinctionAI.MotionData { data = window_jug };
                            var results = mocopiDistinctionAI.RunAI(motionData);

                            // ここでresultsを使用する前にpredictedClassを宣言し、初期化します。
                            // ここでresultsを使用する前にpredictedClassを宣言し、初期化します。
                            int predictedClass = Array.IndexOf(results, results.Max());
                            Debug.Log("Predicted Class: " + predictedClass);

                            // resultの内容を文字列に変換してテキストに追加します
                            string resultString = "Results: ";
                            for (int i = 0; i < results.Length; i++)
                            {
                                resultString += results[i].ToString();
                                if (i < results.Length - 1)
                                {
                                    resultString += ", ";
                                }
                            }
                            Debug.Log("Results: " + resultString);

                            // 予測クラスをテキストとして表示
                            resultText.text = "Predicted Class: " + predictedClass.ToString() + "\n" + resultString;


                            // Prepare for the next data window
                            windowsData.RemoveRange(0, STEP_SIZE);
                        }
                        timer = 0.0f;  // Reset the timer
                    }
                })
                .AddTo(this);
        }
        private List<float> GetMotionData()
        {
            // This method should collect and return the current frame of motion data
            // similar to your original GetMotionData implementation.
            // Here is a simplified version:
            List<float> data = new List<float>();
            // Assume AddTransformData is a method that appends transform data to the list.
                AddTransformData(_rootTransform, data);
                AddTransformData(_lUpLegTransform, data);
                AddTransformData(_rUpLegTransform, data);
                AddTransformData(_lLowLegTransform, data);
                AddTransformData(_rLowLegTransform, data);
                AddTransformData(_lFootTransform, data);
                AddTransformData(_rFootTransform, data);
                AddTransformData(_firstTorsoTransform, data);
                AddTransformData(_thirdTorsoTransform, data);
                AddTransformData(_fifthTorsoTransform, data);
                AddTransformData(_seventhTorsoTransform, data);
                AddTransformData(_rShoulderTransform, data);
                AddTransformData(_lShoulderTransform, data);
                AddTransformData(_firstNeckTransform, data);
                AddTransformData(_secondNeckTransform, data);
                AddTransformData(_rUpArmTransform, data);
                AddTransformData(_lUpArmTransform, data);
                AddTransformData(_rLowArmTransform, data);
                AddTransformData(_lLowArmTransform, data);
                AddTransformData(_rHandTransform, data);
                AddTransformData(_lHandTransform, data);
            // Add data from other transforms similarly...
            return data;
        }
        private void AddTransformData(Transform transform, List<float> data)
        {
            // Append position and rotation data from the transform to the list
            data.Add(transform.localPosition.x);
            data.Add(transform.localPosition.y);
            data.Add(transform.localPosition.z);
            data.Add(transform.localRotation.x);
            data.Add(transform.localRotation.y);
            data.Add(transform.localRotation.z);
            data.Add(transform.localRotation.w);
            // Repeat for other transforms...
        }
        public float[][] GetWindowedData()
        {
            // This method returns the collected windowed data.
            return windowedData.ToArray();
        }
        // listOfLists にデータを追加するコードを記述

        // float[][] jaggedArray = ConvertToJaggedArray(listOfLists);

        // 定義した ConvertToJaggedArray 関数
        public static float[][] ConvertToJaggedArray(List<List<float>> listOfLists)
        {
            int rows = listOfLists.Count;
            int[] columns = listOfLists.Select(list => list.Count).ToArray();

            float[][] jaggedArray = new float[rows][];

            for (int i = 0; i < rows; i++)
            {
                jaggedArray[i] = new float[columns[i]];
                for (int j = 0; j < columns[i]; j++)
                {
                    jaggedArray[i][j] = listOfLists[i][j];
                }
            }

            return jaggedArray;
        }
    }
}


