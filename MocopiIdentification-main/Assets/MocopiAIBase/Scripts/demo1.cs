using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using TMPro;
using OpenAIGPT;
using Cysharp.Threading.Tasks;

namespace MocopiDistinction
{
    internal class demo1 : MonoBehaviour
    {
        [SerializeField] private MocopiDistinctionAI mocopiDistinctionAI;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private string apiKey;

        private GPTConnection _gptConnection;

        // Transform members
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

        private List<List<float>> windowsData = new List<List<float>>();
        private float timer = 0.0f;
        private const float interval = 30.0f;
        private const int WINDOW_SIZE = 20;
        private const int STEP_SIZE = 1;
        private List<float[]> windowedData = new List<float[]>();

        void Start()
        {
            InitializeTransforms();
            _gptConnection = new GPTConnection(apiKey);

            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    timer += Time.deltaTime;
                    if (timer >= interval)
                    {
                        CollectData();
                    }
                })
                .AddTo(this);
        }

        private void InitializeTransforms()
        {
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
        }

        private void CollectData()
        {
            List<float> currentData = GetMotionData();
            windowsData.Add(currentData);
            if (windowsData.Count >= WINDOW_SIZE)
            {
                var window = windowsData.Take(WINDOW_SIZE).ToList();
                var window_jug = ConvertToJaggedArray(window);
                var motionData = new MocopiDistinctionAI.MotionData { data = window_jug };
                var results = mocopiDistinctionAI.RunAI(motionData);

                int predictedClass = Array.IndexOf(results, results.Max());
                Debug.Log("Predicted Class: " + predictedClass);

                GetTextualInterpretation(predictedClass).Forget();

                windowsData.RemoveRange(0, STEP_SIZE);
                timer = 0.0f;
            }
        }

        private List<float> GetMotionData()
        {
            List<float> data = new List<float>();
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
            return data;
        }

        private void AddTransformData(Transform transform, List<float> data)
        {
            data.Add(transform.localPosition.x);
            data.Add(transform.localPosition.y);
            data.Add(transform.localPosition.z);
            data.Add(transform.localRotation.x);
            data.Add(transform.localRotation.y);
            data.Add(transform.localRotation.z);
            data.Add(transform.localRotation.w);
        }

        private static float[][] ConvertToJaggedArray(List<List<float>> listOfLists)
        {
            return listOfLists.Select(a => a.ToArray()).ToArray();
        }

        private async UniTask GetTextualInterpretation(int predictedClass)
        {
            var description = $" 分類結果は{predictedClass}でした。0がジャンプ、1が歩行、2が腕立て伏せです。あなたはスポーツトレーナーです。選手が頑張れるような声掛けをお願いします。 ";
            try
            {
                var response = await _gptConnection.RequestAsync(description);
                var textualResponse = response.choices[0].message.content;
                UpdateUIText(textualResponse);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to get response: " + e.Message);
            }
        }

        private void UpdateUIText(string text)
        {
            resultText.text = text;
        }
    }
}
