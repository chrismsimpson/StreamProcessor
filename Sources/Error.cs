
namespace StreamProcessor;

public class Error {

    public String? Content { get; init; }

    ///

    public Error(
        String? content) {
        
        this.Content = content;
    }
}

///

public partial class ErrorOr<Result> {

    public Result? Value { get; init; }

    public Error? Error { get; init; }

    ///

    public ErrorOr(
        Result? value) {

        this.Value = value;
    }

    public ErrorOr(
        Error e) {

        this.Error = e;
    }

    public ErrorOr(
        String? content) {

        this.Error = new Error(content);
    }
}

public class ErrorOrVoid {

    public Error? Error { get; init; }

    ///

    public ErrorOrVoid() {

        this.Error = null;
    }

    public ErrorOrVoid(
        Error e) {

        this.Error = e;
    }

    public ErrorOrVoid(
        String? content) {

        this.Error = new Error(content);
    }
}
