
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

///

