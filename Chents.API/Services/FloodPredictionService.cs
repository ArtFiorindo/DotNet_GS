using Microsoft.ML;
using Microsoft.ML.Data;
using Chents.Models;
using Chents.Models.Models;

namespace Chents.API.Services;

public class FloodPredictionService
{
    private readonly MLContext _mlContext;
    private ITransformer _model;

    public FloodPredictionService()
    {
        _mlContext = new MLContext();
        
        // In a real app, you would train or load a proper model
        // This is a simplified version for demonstration
        var data = new List<AlertTrainingData>
        {
            new() { Message = "Water rising quickly", City = "São Paulo", Severity = AlertSeverity.High },
            new() { Message = "Minor flooding in streets", City = "Rio de Janeiro", Severity = AlertSeverity.Medium },
            new() { Message = "River overflow", City = "Belo Horizonte", Severity = AlertSeverity.Critical },
            new() { Message = "Damp floors", City = "Curitiba", Severity = AlertSeverity.Low }
        };

        var trainingData = _mlContext.Data.LoadFromEnumerable(data);
        
        var pipeline = _mlContext.Transforms.Text.FeaturizeText("MessageFeatures", "Message")
            .Append(_mlContext.Transforms.Text.FeaturizeText("CityFeatures", "City"))
            .Append(_mlContext.Transforms.Concatenate("Features", "MessageFeatures", "CityFeatures"))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", "Severity"))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        _model = pipeline.Fit(trainingData);
    }

    public AlertSeverity PredictSeverity(string message, string city)
    {
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<AlertTrainingData, AlertPrediction>(_model);
        
        var prediction = predictionEngine.Predict(new AlertTrainingData
        {
            Message = message,
            City = city
        });

        return prediction.PredictedSeverity;
    }
}

public class AlertTrainingData
{
    public string Message { get; set; }
    public string City { get; set; }
    public AlertSeverity Severity { get; set; }
}

public class AlertPrediction
{
    [ColumnName("PredictedLabel")]
    public AlertSeverity PredictedSeverity { get; set; }
}