
namespace StreamProcessor;

public partial class StreamItem {

    public String Type { get; init; }

    public String TokenId { get; init; }

    public String? Address { get; init; }

    public String? From { get; init; }

    public String? To { get; init; }

    ///

    public StreamItem(
        String type,
        String tokenId,
        String? address,
        String? from,
        String? to) {

        this.Type = type;
        this.TokenId = tokenId;
        this.Address = address;
        this.From = from;
        this.To = to;
    }
}

public static partial class StreamItemFunctions {

    public static ErrorOr<IStreamEvent> ToStreamEvent(
        this StreamItem item) {

        switch (item.Type) {

            case "Mint" when 
                item.Address is String address: {

                return new ErrorOr<IStreamEvent>(
                    new Mint(item.TokenId, address));
            }

            case "Mint": {

                return new ErrorOr<IStreamEvent>("incomplete mint event");
            }

            case "Burn": {

                return new ErrorOr<IStreamEvent>(
                    new Burn(item.TokenId));
            }

            case "Transfer" when
                item.From is String from
                && item.To is String to: {

                return new ErrorOr<IStreamEvent>(
                    new Transfer(item.TokenId, item.From, item.To));
            }

            case "Transfer": {

                return new ErrorOr<IStreamEvent>("incomplete transfer event");
            }

            default: {

                return new ErrorOr<IStreamEvent>("invalid event");
            }
        }
    }

    public static (List<IStreamEvent>, Error?) ToStreamEvents(
        this List<StreamItem> items) {

        Error? error = null;

        var events = new List<IStreamEvent>();

        for (var index = 0; index < items.Count; index++) {

            var item = items[index];

            var eventOrError = item.ToStreamEvent();

            if (eventOrError.Error is not null 
                || eventOrError.Value is null) {

                error = error ??
                    new Error(
                        !IsNullOrWhiteSpace(eventOrError.Error?.Content)
                            ? $"error at index {index}: {eventOrError.Error?.Content}"
                            : $"unknown error at index: {index}");

                continue;
            }

            events.Add(eventOrError.Value);
        }
        
        return (events, error);
    }
}

///

public enum StreamEventType {

    Mint,
    Burn,
    Transfer
}

public interface IStreamEvent {

    StreamEventType Type { get; }

    String TokenId { get; }
}

///

public sealed class Mint: IStreamEvent {

    public StreamEventType Type => StreamEventType.Mint;

    public String TokenId { get; init; }

    public String Address { get; init; }

    ///

    public Mint(
        String tokenId,
        String address) {

        this.TokenId = tokenId;
        this.Address = address;
    }
}

public sealed class Burn: IStreamEvent {

    public StreamEventType Type => StreamEventType.Burn;

    public String TokenId { get; init; }

    ///

    public Burn(
        String tokenId) {

        this.TokenId = tokenId;
    }
}

public sealed class Transfer: IStreamEvent {

    public StreamEventType Type => StreamEventType.Transfer;

    public String TokenId { get; init; }

    public String From { get; init; }

    public String To { get; init; }

    ///

    public Transfer(
        String tokenId,
        String from,
        String to) {

        this.TokenId = tokenId;
        this.From = from;
        this.To = tokenId;
    }
}