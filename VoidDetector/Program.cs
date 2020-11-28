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
        static readonly string _testTagsTsv = Path.Combine(_imagesFolder, "test-tags.tsv");
        static readonly string _predictSingleImage = Path.Combine(_imagesFolder, "t485. 11.19.00.jpg");
        static readonly string _inceptionTensorFlowModel = Path.Combine(_assetsPath, "inception", "tensorflow_inception_graph.pb");
        
        static void Main(string[] args)
        {
            //MLContext mlContext = new MLContext();

            //ITransformer model = GenerateModel(mlContext);

            //ClassifySingleImage(mlContext, model);

            //CutImage(@"C:\Users\Tibi\Desktop\hack\fotos BonArr",0,0,1920,1080);

            Lineal lineal = new Lineal();
            List<Sector> sectors = GetXY();

            lineal.sectors = sectors;
            lineal.imagePath = _predictSingleImage;
            
            GenerateImageToPredict(lineal);

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

        public static void ClassifySingleImage(MLContext mlContext, ITransformer model)
        {
            var imageData = new ImageData()
            {
                ImagePath = _predictSingleImage
            };

            // Make prediction function (input = ImageData, output = ImagePrediction)
            var predictor = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model);
            var prediction = predictor.Predict(imageData);

            Console.WriteLine($"Image: {Path.GetFileName(imageData.ImagePath)} predicted as: {prediction.PredictedLabelValue} with score: {prediction.Score.Max()} ");
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

        private static void GenerateImageToPredict(Lineal lineal)
        {
            try
            {
                RemoveOldImages();

                foreach (Sector sec in lineal.sectors)
                {
                    Rectangle cropRect = new Rectangle(sec.x, sec.y, sec.width, sec.height);

                    Bitmap src = Image.FromFile(lineal.imagePath) as Bitmap;
                    Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                    //using (Graphics graphics = Graphics.FromImage(target))
                    //{
                    //    using (System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
                    //    {
                    //        graphics.FillRectangle(myBrush, new Rectangle(0, 0, 200, 300));

                    //    }
                    //    target.Save("test");
                    //}

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

            for (int i = 1; i<21; i++)
            {
                if (i<=13)
                {
                    sectors.Add(new Sector("A" + i));
                    sectors.Add(new Sector("B" + i));
                    sectors.Add(new Sector("C" + i));
                    sectors.Add(new Sector("D" + i));
                    sectors.Add(new Sector("E" + i));
                    continue;
                }

                if (i<=15)
                {
                    sectors.Add(new Sector("A" + i));
                    sectors.Add(new Sector("B" + i));
                    sectors.Add(new Sector("C" + i));
                    sectors.Add(new Sector("D" + i));
                    continue;
                }

                if (i<=16)
                {
                    sectors.Add(new Sector("B" + i));
                    sectors.Add(new Sector("C" + i));
                    continue;
                }

                if (i<=20)
                {
                    sectors.Add(new Sector("B" + i));
                    continue;
                }
            }
           
            return sectors;

        }
    }
}
