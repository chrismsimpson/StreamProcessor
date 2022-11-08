
namespace StreamProcessor;

public static partial class Program {

    public static String? ValueForKey(
        this String[] args,
        String key) {

        for (var index = 0; index < args.Length; index++) {

            var arg = args[index];

            if (arg != key) {

                continue;
            }

            var nextIndex = index + 1;

            if (nextIndex < args.Length) {

                return args[nextIndex];
            }
        }

        return null;
    }

    public static void WriteErrorLine(
        String message) {

        WriteLine($"sp: error: {message}");
    }

    ///

    public static ErrorOr<List<StreamItem>> GetStreamItemsFromFilename(
        String filename) {

        var contents = File.ReadAllText(filename);

        if (IsNullOrWhiteSpace(contents)) {

            return new ErrorOr<List<StreamItem>>($"file '{filename}' is empty");
        }

        return GetStreamItemsFromContents(contents, filename);
    }

    public static ErrorOr<List<StreamItem>> GetStreamItemsFromContents(
        String contents,
        String? filename = null) {

        var _contents = contents;
        
        var peek = _contents.FirstOrDefault();

        if (peek == '\'' 
            && contents.LastOrDefault() == '\'') {

            // Escape any single quotes

            _contents = _contents.Substring(1, _contents.Length - 2);
        
            peek = _contents.FirstOrDefault();
        }

        switch (peek) {

            case '[': {

                var items = JsonSerializer.Deserialize<List<StreamItem>>(_contents);

                return new ErrorOr<List<StreamItem>>(items);
            }

            case '{': {

                var item = JsonSerializer.Deserialize<StreamItem>(_contents);

                if (item is null) {

                    return new ErrorOr<List<StreamItem>>(
                        !IsNullOrWhiteSpace(filename)
                            ? $"could not deserialize item from file '{filename}'"
                            : "could not deserialize item from input");
                } 

                return new ErrorOr<List<StreamItem>>(new List<StreamItem>(new StreamItem[] { item }));
            }

            default: {

                return new ErrorOr<List<StreamItem>>(
                    !IsNullOrWhiteSpace(filename)
                        ? $"'{filename}' does not appear to be valid json"
                        : "input does not appear to be valid json");
            }
        }
    }

    ///

    public static (List<IStreamEvent>, Error?) GetStreamEventsFromItems(
        ErrorOr<List<StreamItem>> errorOrItems) {

        if (errorOrItems.Error is not null) {

            return (new (), errorOrItems.Error);
        }

        if (errorOrItems.Value is null) {

            return (new (), null);
        }

        return errorOrItems.Value.ToStreamEvents();
    }

    public static (List<IStreamEvent>, Error?) GetStreamEventsFromFilename(
        String filename) {

        return GetStreamEventsFromItems(
            GetStreamItemsFromFilename(filename));
    }

    public static (List<IStreamEvent>, Error?) GetStreamEventsFromContents(
        String contents,
        String? filename = null) {

        return GetStreamEventsFromItems(
            GetStreamItemsFromContents(contents, filename));
    }

    ///

    public static ErrorOrVoid ReadEventsIntoLedger(
        String filename,
        List<IStreamEvent> events) {

        var path = Path.GetDirectoryName(filename);

        if (!IsNullOrWhiteSpace(path) 
            && path != ".") {

            if (!Directory.Exists(path)) {

                Directory.CreateDirectory(path);
            }
        }

        ///

        var ledgerOrError = OpenOrCreateLedger(filename);

        if (ledgerOrError.Error is not null
            || ledgerOrError.Value is null) {

            return new ErrorOrVoid(ledgerOrError.Error?.Content ?? "unknown error");
        }

        var ledger = ledgerOrError.Value;

        ///

        var (items, err) = events.ToStreamItems();

        if (err is not null) {

            return new ErrorOrVoid(err.Content ?? "unknown error");
        }

        if (!items.Any()) {

            // nothing to do

            return new ErrorOrVoid();
        }

        foreach (var i in items) {

            ledger.Add(i);
        }

        File.WriteAllText(
            filename, 
            JsonSerializer.Serialize(
                ledger, 
                new JsonSerializerOptions { WriteIndented = true }));

        return new ErrorOrVoid();
    }

    public static ErrorOr<List<StreamItem>> OpenOrCreateLedger(
        String filename) {

        List<StreamItem> store;

        if (!File.Exists(filename)) {

            store = new List<StreamItem>();

            File.WriteAllText(filename, JsonSerializer.Serialize(store));
        }
        else {

            var contents = File.ReadAllText(filename);

            if (IsNullOrWhiteSpace(contents)) {

                return new ErrorOr<List<StreamItem>>("ledger is empty");
            }

            var itemsOrError = GetStreamItemsFromFilename(filename);

            if (itemsOrError.Error is not null
                || itemsOrError.Value is null) {

                return new ErrorOr<List<StreamItem>>(itemsOrError.Error?.Content ?? "unknown error");
            }

            store = itemsOrError.Value;
        }

        return new ErrorOr<List<StreamItem>>(store);
    }

    ///

    public static int Main(String[] args) {

        var ledgerFilename = args.ValueForKey("--ledger") ?? "./Store/ledger.json";
        // var ledgerFilename = args.ValueForKey("--ledger") ?? "./ledger.json";

        ///

        switch (true) {

            case var _ when
                args.ValueForKey("--read-file") is String filename: {

                if (!File.Exists(filename)) {

                    WriteErrorLine($"file '{filename}' does not exist");

                    return 1;
                }

                var (events, err) = GetStreamEventsFromFilename(filename);

                if (err is not null) {

                    WriteErrorLine(err.Content ?? "unknown error");

                    return 1;
                }

                ReadEventsIntoLedger(ledgerFilename, events);

                return 0;
            }

            case var _ when
                args.ValueForKey("--read-inline") is String inline: {

                var (events, err) = GetStreamEventsFromContents(inline);

                if (err is not null) {

                    WriteErrorLine(err.Content ?? "unknown error");

                    return 1;
                }

                ReadEventsIntoLedger(ledgerFilename, events);

                return 0;
            }

            case var _ when
                args.ValueForKey("--nft") is String nftId: {

                // TODO: id validation?
                
                WriteLine($"TODO: nft id");

                return 0;
            }

            case var _ when
                args.ValueForKey("--wallet") is String address: {
                
                // TODO: address validation?

                WriteLine($"TODO: wallet address");

                return 0;
            }

            case var _ when
                args.ValueForKey("--reset") is String address: {
                
                // TODO: address validation?

                WriteLine($"TODO: reset");

                return 0;
            }
            
            default: {

                WriteLine($"TODO: write usage line");

                return 1;
            }
        }
    }
}