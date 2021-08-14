using System.Collections.Generic;

namespace UnsplashAPI.Modules {
  public class Response {
    public object Headers { get; set; }

    public List<ErrorMessage> Errors { get; set; }

    public bool Ok { get; set; }

    public int Status { get; set; }

    public string StatusText { get; set; }

    public string Type { get; set; }

    public string Title { get; set; }

    public string TraceId { get; set; }

    /* Functions */
    public void LoadData(bool ok = true, int status = 200, object headers = null, List<ErrorMessage> errors = null) {
      Ok = ok;
      Status = status;
      StatusText = status.ToString();
      Headers = headers;
      Errors = errors;
    }

    public void StatusCode200(object headers) {
      LoadData(true, 200, headers, new List<ErrorMessage>());
    }
    
    public void StatusCode400(params ErrorMessage[] errors) {
      List<ErrorMessage> errorsList = new();

      foreach(var error in errors) {
        errorsList.Add(error);
      }

      LoadData(false, 400, errors: errorsList);
    }

    public void StatusCode400(List<ErrorMessage> errors) 
      => LoadData(false, 400, errors: errors);

    public void StatusCode401(params ErrorMessage[] errors) {
      List<ErrorMessage> errorsList = new();

      foreach(var error in errors) {
        errorsList.Add(error);
      }

      LoadData(false, 401, errors: errorsList);
    }
  }

  public class ErrorMessage {

    public ErrorMessage(string code, string description) {
      Code = code;
      Description = description;
    }
    public string Code { get; set; }
    public string Description { get; set; }
  }
}