using System;
using Unity.Sentis;
using System.Linq; // これを追加してください
using UnityEngine;
namespace MocopiDistinction
{
    /// <summary>
    /// AIの演算を行うクラス
    /// </summary>
    internal class MocopiDistinctionAI : MonoBehaviour
    {
        [Header("ONNXモデル"),SerializeField]
        private ModelAsset nnModel = null;
        private Model _runtimeModel;
        private TensorFloat _inputTensor;
        private IWorker _engine;
        private TensorShape shape = new(1,20, 147);
        public class MotionData
        {
            public float[][] data;
        }
        private void Awake()
        {
            if (nnModel == null)
            {
                Debug.LogError("ONNXモデルを設定してください");
            }
            _runtimeModel = ModelLoader.Load(nnModel);
        }
// MocopiDistinctionAI クラス内

        public float[] RunAI(MotionData motionData)
        {
            _engine = WorkerFactory.CreateWorker(BackendType.CPU, _runtimeModel);

            // 入力データの形状を設定します。
            int dataLength = motionData.data.Length;
            int dataDepth = motionData.data[0].Length;
            float[] flatData = new float[dataLength * dataDepth];
            int index = 0;
            foreach (var row in motionData.data)
            {
                foreach (var val in row)
                {
                    flatData[index++] = val;
                }
            }

            // BarracudaのTensorを作成し、データを設定します。
            Tensor inputTensor = new TensorFloat(shape, flatData);

            // 推論を実行します。
            _engine.Execute(inputTensor);
            TensorFloat outputTensor = _engine.PeekOutput() as TensorFloat;
            outputTensor.MakeReadable();
            var results = outputTensor.ToReadOnlyArray() ?? throw new ArgumentNullException("outputTensor.ToReadOnlyArray()");
            _engine.Dispose();
            inputTensor.Dispose();
            outputTensor.Dispose();
            return results;
        }

    }
}
