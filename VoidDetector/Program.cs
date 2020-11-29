using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace VoidDetector
{
    public class Program
    {
      
        static readonly string _myProjectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..\\VoidDetector"));
        static readonly string _assetsPath = Path.Combine(_myProjectPath, "assets");
        static readonly string _imageToProcessFolder = Path.Combine(_assetsPath, "imatgesToProcess");
      
        static readonly string _imagesFolder = Path.Combine(_assetsPath, "images");
        static readonly string _fullFolder = Path.Combine(_assetsPath, "full");
        static readonly string _emptyFolder = Path.Combine(_assetsPath, "empty");
        static readonly string _obstructionFolder = Path.Combine(_assetsPath, "obstruction");

        static readonly string _trainTagsTsv = Path.Combine(_assetsPath, "tags.tsv");
        static readonly string _predictSingleImage = Path.Combine(_imagesFolder, "t485. 09.20.53.jpg");


        //static readonly string _predictSingleImage = Path.Combine(_imagesFolder, "t485. 09.20.53.jpg");
        //static readonly string _imagesWithSquare = Path.Combine(_imagesFolder, "imagesWithSquare");

        static readonly string _inceptionTensorFlowModel = Path.Combine(_assetsPath, "inception", "tensorflow_inception_graph.pb");

        static List<string> _sectors;
        static Lineal lineal = new Lineal();

        public static List<Results> Start(string file)
        {
            file = Path.Combine(_imagesFolder, file);

            _sectors = GetSectors();
            GenerateImageToPredict(file);

            MLContext mlContext = new MLContext();
            ITransformer model = GenerateModel(mlContext);

            List<Results> results = ClassifyImage(mlContext, model);

            //PrintResults(results);
            DrawSquares(results, file);

            return results;

        }
        static void Main(string[] args)
        {

            //_sectors = GetSectors();
            //GenerateImageToPredict();
          
            //List<Results> results = ClassifyImage(mlContext, model);

            //PrintResults(results);
            //DrawSquares(results);

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
            IEstimator<ITransformer> pipeline = mlContext.Transforms.LoadImages(outputColumnName: "input", imageFolder: _assetsPath, inputColumnName: nameof(ImageData.ImagePath))
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
            List<Results> res = new List<Results>();


            System.IO.DirectoryInfo di = new DirectoryInfo(_imageToProcessFolder);
            //RemoveOldImages();

            foreach (FileInfo fileImage in di.GetFiles())
            {
                Console.WriteLine("Classifing " + fileImage.FullName);
                var imageData = new ImageData()
                {
                    ImagePath = fileImage.FullName
                };

                var predictor = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model);
                var prediction = predictor.Predict(imageData);
                res.Add(new Results { name = fileImage.Name, prediction = prediction.PredictedLabelValue, score = (prediction.Score.Max() * 100) });
                Console.WriteLine($"Image: {Path.GetFileName(imageData.ImagePath)} predicted as: {prediction.PredictedLabelValue} with score: {prediction.Score.Max()} ");
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

        public static void DrawSquares(List<Results> results, string file)
        {
            try
            {
                results = results.Where(x => x.prediction == "empty").ToList();

                Bitmap src = Image.FromFile(file) as Bitmap;
                Pen pen = new Pen(Color.Red, 2);

                Bitmap target = new Bitmap(1920, 1080);

                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height), new Rectangle(0, 0, target.Width, target.Height), GraphicsUnit.Pixel);
                }

                foreach (Results res in results)
                {
                    int x = lineal.sectors.Where(x => x.nomSector == res.name + ".jpg").Select(y => y.x).FirstOrDefault();
                    int y = lineal.sectors.Where(x => x.nomSector == res.name + ".jpg").Select(y => y.y).FirstOrDefault();

                    using (Graphics g = Graphics.FromImage(target))
                    {
                        g.DrawRectangle(pen, new Rectangle(x, y, 60, 60));
                    }

                }
                string path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\wwwroot\\img\\resultat.jpg"));
                target.Save(path);

                Console.WriteLine("a pintat gg");
            }
            catch (Exception ex)
            {
                Console.WriteLine("no pinta...");
            }

        }

        private static void GenerateImageToPredict(string file)
        {
            try
            {
                List<Sector> sectors = GetXY();

                lineal.sectors = sectors;
                lineal.imagePath = file;

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

        private static void GenerateMultipleImageToPredict()
        {
            try
            {
                List<Sector> sectors = GetXY();

                System.IO.DirectoryInfo di = new DirectoryInfo(_imagesFolder);
                RemoveOldImages();

                foreach (FileInfo fileImage in di.GetFiles())
                {
                    Console.WriteLine("Procesing image " + fileImage.FullName); 

                    lineal.sectors = sectors;
                    lineal.imagePath = fileImage.FullName;

                    Bitmap src = Image.FromFile(lineal.imagePath) as Bitmap;

                    foreach (Sector sec in sectors)
                    {
                        Rectangle cropRect = new Rectangle(sec.x, sec.y, sec.width, sec.height);
                        using Bitmap target = new Bitmap(cropRect.Width, cropRect.Height) as Bitmap;

                        using (Graphics g = Graphics.FromImage(target))
                        {
                            g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                             cropRect,
                                             GraphicsUnit.Pixel);
                        }

                        target.Save(_myProjectPath + "\\assets\\imatgesToProcess\\" + Path.GetFileNameWithoutExtension(fileImage.Name) + "_" + sec.nomSector);
                    }
                }
                Console.WriteLine("Finished Generating Images");
            }
            catch (Exception ex)
            {
                Console.WriteLine("burro "+ ex.Message);
            }

        }

        private static void CreateTagList()
        {
            try
            {
                Console.WriteLine("Generating Tag File");
                StringBuilder sb_tags = new StringBuilder();
                System.IO.DirectoryInfo full_dir = new DirectoryInfo(_fullFolder);
                System.IO.DirectoryInfo empty_dir = new DirectoryInfo(_emptyFolder);
                System.IO.DirectoryInfo obstruction_dir = new DirectoryInfo(_obstructionFolder);

                foreach (FileInfo file in full_dir.GetFiles())
                {
                    sb_tags.AppendLine("full\\" + file.Name + "\t" + "full");
                }
                foreach (FileInfo file in empty_dir.GetFiles())
                {
                    sb_tags.AppendLine("empty\\" + file.Name + "\t" + "empty");
                }
                foreach (FileInfo file in obstruction_dir.GetFiles())
                {
                    sb_tags.AppendLine("obstruction\\" + file.Name + "\t" + "obstruction");
                }
                System.IO.File.WriteAllText(_trainTagsTsv, sb_tags.ToString());

                Console.WriteLine("Tag File Generated");
            }
            catch (Exception ex)
            {
                Console.WriteLine("burro " + ex.Message);
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

            foreach (string sec in _sectors)
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
    }
}
