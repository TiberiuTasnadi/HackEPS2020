using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace VoidDetector
{
    class Program
    {

        static readonly string _myProjectPath = "D:\\Code\\HackEPS2020\\VoidDetector";
        static readonly string _imageToProcessFolder = _myProjectPath + "\\assets\\imatgesToProcess";
        static readonly string _assetsPath = _myProjectPath + "\\assets";

        static readonly string _imagesFolder = Path.Combine(_assetsPath, "images");
        static readonly string _trainTagsTsv = Path.Combine(_imagesFolder, "tags.tsv");
        static readonly string _predictSingleImage = Path.Combine(_imagesFolder, "t485. 20.28.54.jpg");
        
        static readonly string _inceptionTensorFlowModel = Path.Combine(_assetsPath, "inception", "tensorflow_inception_graph.pb");

        static List<string> _sectors;

        static void Main(string[] args)
        {

            _sectors = GetSectors();
            GenerateImageToPredict();

            MLContext mlContext = new MLContext();
            ITransformer model = GenerateModel(mlContext);

            List<Results> results = ClassifyImage(mlContext, model);

            PrintResults(results);

            Console.Read();

        }

        public class ImageData
        {
            [LoadColumn(0)]
            public string ImagePath;

            [LoadColumn(1)]
            public string Label;
        }

        public class ImagePrediction : ImageData
        {
            public float[] Score;

            public string PredictedLabelValue;
        }

        private struct InceptionSettings
        {
            public const int ImageHeight = 224;
            public const int ImageWidth = 224;
            public const float Mean = 117;
            public const float Scale = 1;
            public const bool ChannelsLast = true;
        }

        public static ITransformer GenerateModel(MLContext mlContext)
        {
            IEstimator<ITransformer> pipeline = mlContext.Transforms.LoadImages(outputColumnName: "input", imageFolder: _imagesFolder, inputColumnName: nameof(ImageData.ImagePath))
                            // The image transforms transform the images into the model's expected format.
                            .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input", imageWidth: InceptionSettings.ImageWidth, imageHeight: InceptionSettings.ImageHeight, inputColumnName: "input"))
                            .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: InceptionSettings.ChannelsLast, offsetImage: InceptionSettings.Mean))
                            .Append(mlContext.Model.LoadTensorFlowModel(_inceptionTensorFlowModel)
                            .ScoreTensorFlowModel(outputColumnNames: new[] { "softmax2_pre_activation" }, inputColumnNames: new[] { "input" }, addBatchDimensionInput: true))
                            .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelKey", inputColumnName: "Label"))
                            .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: "LabelKey", featureColumnName: "softmax2_pre_activation"))
                            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabelValue", "PredictedLabel"))
                            .AppendCacheCheckpoint(mlContext);

            IDataView trainingData = mlContext.Data.LoadFromTextFile<ImageData>(path: _trainTagsTsv, hasHeader: false);
            ITransformer model = pipeline.Fit(trainingData);

            //IDataView testData = mlContext.Data.LoadFromTextFile<ImageData>(path: _testTagsTsv, hasHeader: false);
            //IDataView predictions = model.Transform(testData);

            //// Create an IEnumerable for the predictions for displaying results
            //IEnumerable<ImagePrediction> imagePredictionData = mlContext.Data.CreateEnumerable<ImagePrediction>(predictions, true);
            //DisplayResults(imagePredictionData);

            //MulticlassClassificationMetrics metrics = mlContext.MulticlassClassification.Evaluate(predictions, labelColumnName: "LabelKey", predictedLabelColumnName: "PredictedLabel");

            //Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
            //Console.WriteLine($"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");

            return model;

        }

        private static void DisplayResults(IEnumerable<ImagePrediction> imagePredictionData)
        {
            foreach (ImagePrediction prediction in imagePredictionData)
            {
                Console.WriteLine($"Image: {Path.GetFileName(prediction.ImagePath)} predicted as: {prediction.PredictedLabelValue} with score: {prediction.Score.Max()} ");
            }
        }

        public static IEnumerable<ImageData> ReadFromTsv(string file, string folder)
        {

            return File.ReadAllLines(file).Select(line => line.Split('\t')).Select(line => new ImageData()
            {
                ImagePath = Path.Combine(folder, line[0])
            });
        }

        public static List<Results> ClassifyImage(MLContext mlContext, ITransformer model)
        {
            List<Results>res = new List<Results>();
            foreach (string sec in _sectors)
            {
                var imageData = new ImageData()
                {
                    ImagePath = _imageToProcessFolder + "\\" + sec + ".jpg"
                };
               
                var predictor = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model);
                var prediction = predictor.Predict(imageData);
                res.Add(new Results { name = sec, prediction = prediction.PredictedLabelValue , score = (prediction.Score.Max()*100)});
                //Console.WriteLine($"Image: {Path.GetFileName(imageData.ImagePath)} predicted as: {prediction.PredictedLabelValue} with score: {prediction.Score.Max()} ");
            }
            return res;
        }

        public static void PrintResults(List<Results> results)
        {
            Console.WriteLine("**************************************************************************");
            results = results.OrderBy(x => x.score).ToList();
            foreach (Results res in results)
            {
                Console.WriteLine(res.name + "\t" + res.prediction + "\t" + res.score);
            }
        }

        private static void GenerateImageToPredict()
        {
            try
            {
                Lineal lineal = new Lineal();
                List<Sector> sectors = GetXY();

                lineal.sectors = sectors;
                lineal.imagePath = _predictSingleImage;

                RemoveOldImages();

                foreach (Sector sec in lineal.sectors)
                {
                    Rectangle cropRect = new Rectangle(sec.x, sec.y, sec.width, sec.height);

                    Bitmap src = Image.FromFile(lineal.imagePath) as Bitmap;
                    Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                    using (Graphics g = Graphics.FromImage(target))
                    {
                        g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                         cropRect,
                                         GraphicsUnit.Pixel);
                    }

                    target.Save(_myProjectPath + "\\assets\\imatgesToProcess\\" + sec.nomSector);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("burro");
            }

        }

        private static void RemoveOldImages()
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(_imageToProcessFolder);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        private static List<Sector> GetXY()
        {
            List<Sector> sectors = new List<Sector>();
            
            foreach(string sec in _sectors)
            {
                sectors.Add(new Sector(sec));
            }

            return sectors;

        }

        private static List<string> GetSectors()
        {
            List<string> sectors = new List<string>();
            for (int i = 1; i < 21; i++)
            {
                if (i <= 13)
                {
                    sectors.Add("A" + i);
                    sectors.Add("B" + i);
                    sectors.Add("C" + i);
                    sectors.Add("D" + i);
                    sectors.Add("E" + i);
                    continue;
                }

                if (i <= 15)
                {
                    sectors.Add("A" + i);
                    sectors.Add("B" + i);
                    sectors.Add("C" + i);
                    sectors.Add("D" + i);
                    continue;
                }

                if (i <= 16)
                {
                    sectors.Add("B" + i);
                    sectors.Add("C" + i);
                    continue;
                }

                if (i <= 20)
                {
                    sectors.Add("B" + i);
                    continue;
                }
            }
            return sectors;
        }       
       
        public class Results
        {
            public string name { get; set; }
            public string prediction { get; set; }
            public float score { get; set; }
        }

    }
}
