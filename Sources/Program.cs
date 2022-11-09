
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

        var og = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.Red;

        WriteLine($"sp: error: {message}");
        
        Console.ForegroundColor = og;
    }

    ///

    public static ErrorOr<List<StreamItem>> OpenOrCreateStream(
        String filename) {

        var path = Path.GetDirectoryName(filename);

        if (!IsNullOrWhiteSpace(path) 
            && path != ".") {

            if (!Directory.Exists(path)) {

                Directory.CreateDirectory(path);
            }
        }

        ///

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

    public static void ResetStream(
        String filename) {

        if (File.Exists(filename)) {

            File.WriteAllText(filename, "[]");
        }
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

    public static ErrorOr<(List<StreamItem> Items, Dictionary<String, String> Ledger)> OpenOrCreateLedger(
        String filename) {

        var streamOrError = OpenOrCreateStream(filename);

        if (streamOrError.Error is not null
            || streamOrError.Value is null) {

            return new ErrorOr<(List<StreamItem>, Dictionary<String, String>)>(streamOrError.Error?.Content ?? "unknown error");
        }

        var stream = streamOrError.Value;

        ///

        var ledger = new Dictionary<String, String>();

        for (var index = 0; index < stream.Count; index++) {

            var item = stream[index];

            var eventOrError = item.ToStreamEvent();

            if (eventOrError.Error is not null
                || eventOrError.Value is null) {

                return new ErrorOr<(List<StreamItem>, Dictionary<String, String>)>(
                    !IsNullOrWhiteSpace(eventOrError.Error?.Content)
                        ? $"error in ledger at index {index}: {eventOrError.Error?.Content}"
                        : $"unknown error in ledger at index {index}");
            }

            var err = ledger.ProcessEvent(eventOrError.Value);

            if (err is not null) {

                return new ErrorOr<(List<StreamItem>, Dictionary<String, String>)>(
                    !IsNullOrWhiteSpace(err.Content)
                        ? $"error loading ledger at index {index}: {err.Content}"
                        : $"unknown error loading ledger at index {index}");
            }
        }

        ///

        return new ErrorOr<(List<StreamItem>, Dictionary<String, String>)>((stream, ledger));
    }

    /// 

    public static Error? ProcessEvent(
        this Dictionary<String, String> ledger,
        IStreamEvent e) {

        switch (e) {

            case Mint mint: {

                if (ledger.ContainsKey(mint.TokenId)) {

                    return new Error($"attempt to mint an existing token");
                }

                ledger[mint.TokenId] = mint.Address;

                return null;
            }

            case Burn burn: {

                if (!ledger.ContainsKey(burn.TokenId)) {

                    return new Error($"attempt to burn non existant token");
                }

                ledger.Remove(burn.TokenId);

                return null;
            }

            case Transfer transfer: {

                if (!ledger.ContainsKey(transfer.TokenId)) {

                    return new Error($"attempt to transfer non existant token");
                }

                var currentOwner = ledger[transfer.TokenId];

                if (currentOwner != transfer.From) {

                    return new Error($"attempt to transfer unowned token");
                }

                ledger[transfer.TokenId] = transfer.To;

                return null;
            }

            default: {

                return new Error($"unknown event type");
            }
        }
    }

    ///

    public static ErrorOr<int> ReadEventsIntoLedgerOrError(
        String ledgerFilename,
        List<IStreamEvent> events) {

        var ledgerOrError = OpenOrCreateLedger(ledgerFilename);

        if (ledgerOrError.Error is not null) {

            return new ErrorOr<int>(ledgerOrError.Error?.Content ?? "unknown error");
        }

        ///

        var (stream, ledger) = ledgerOrError.Value;

        ///

        var startCount = stream.Count;

        ///

        var (items, streamError) = events.ToStreamItems();

        if (streamError is not null) {

            return new ErrorOr<int>(streamError.Content ?? "unknown error");
        }

        if (!items.Any()) {

            // nothing to do

            return new ErrorOr<int>(0);
        }

        ///

        for (var index = 0; index < events.Count; index++) {

            var e = events[index];

            ///

            var itemOrError = e.ToStreamItem();

            if (itemOrError.Error is not null
                || itemOrError.Value is null) {

                return new ErrorOr<int>(
                    !IsNullOrWhiteSpace(itemOrError.Error?.Content)
                        ? $"error re-serializing event at index {index}: {itemOrError.Error?.Content}"
                        : $"unknown error re-serializing event at index {index}");
            }

            var item = itemOrError.Value;

            ///

            var ledgerError = ledger.ProcessEvent(e);

            if (ledgerError is not null) {

                return new ErrorOr<int>(
                    !IsNullOrWhiteSpace(ledgerError.Content)
                    ? $"error processing incoming event at index {index}: {ledgerError.Content}"
                    : $"unknown error processing incoming event at index {index}");
            }

            ///

            stream.Add(item);
        }

        ///

        File.WriteAllText(
            ledgerFilename, 
            JsonSerializer.Serialize(
                stream, 
                new JsonSerializerOptions { WriteIndented = true }));

        return new ErrorOr<int>(stream.Count - startCount);
    }

    public static int ReadEventsIntoLedger(
        String ledgerFilename,
        (List<IStreamEvent> Events, Error? Error) eventsOrError) {

        if (eventsOrError.Error is not null) {

            WriteErrorLine(eventsOrError.Error.Content ?? "unknown error");

            return 1;
        }

        var readOrError = ReadEventsIntoLedgerOrError(ledgerFilename, eventsOrError.Events);

        if (readOrError.Error is not null) {

            WriteErrorLine(
                !IsNullOrWhiteSpace(readOrError.Error?.Content)
                ? $"error reading events into ledger: {readOrError.Error?.Content}"
                : $"unknown error reading events into ledger");

            return 1;
        }

        WriteLine($"Read {readOrError.Value} transaction(s)");

        return 0;
    }

    ///

    public static int Main(String[] args) {

        var ledgerFilename = args.ValueForKey("--ledger") ?? "./Store/ledger.json";

        ///

        switch (true) {

            case var _ when
                args.ValueForKey("--read-file") is String filename: {

                if (!File.Exists(filename)) {

                    WriteErrorLine($"file '{filename}' does not exist");

                    return 1;
                }

                return ReadEventsIntoLedger(ledgerFilename, GetStreamEventsFromFilename(filename));
            }

            case var _ when
                args.ValueForKey("--read-inline") is String inline: {

                return ReadEventsIntoLedger(ledgerFilename, GetStreamEventsFromContents(inline));
            }

            case var _ when
                args.ValueForKey("--nft") is String tokenId: {

                var ledgerOrError = OpenOrCreateLedger(ledgerFilename);

                if (ledgerOrError.Error is not null) {

                    WriteLine($"error opening ledger file '{ledgerFilename}'");

                    return 1;
                }

                var (_, ledger) = ledgerOrError.Value;

                ///

                if (ledger.ContainsKey(tokenId)) {

                    WriteLine($"Token {tokenId} is owned by {ledger[tokenId]}");
                }
                else {

                    WriteLine($"Token {tokenId} is not owned by any wallet");
                }

                return 0;
            }

            case var _ when
                args.ValueForKey("--wallet") is String address: {

                var ledgerOrError = OpenOrCreateLedger(ledgerFilename);

                if (ledgerOrError.Error is not null) {

                    WriteLine($"error opening ledger file '{ledgerFilename}'");

                    return 1;
                }

                var (_, ledger) = ledgerOrError.Value;

                ///

                var entries = ledger
                    .Where(x => x.Value == address)
                    .ToList();

                if (!entries.Any()) {

                    WriteLine($"Wallet {address} holds no Tokens");
                }
                else {

                    WriteLine($"Wallet {address} holds {entries.Count} Token{(entries.Count == 1 ? "" : "s")}:");
                }

                foreach (var entry in entries) {

                    WriteLine(entry.Key);
                }

                return 0;
            }

            case var _ when
                args.Contains("--reset"): {

                ResetStream(ledgerFilename);
                
                WriteLine($"Program was reset");

                return 0;
            }
            
            default: {

                WriteLine(USAGE);

                return 1;
            }
        }
    }

    public static readonly String USAGE = 
@"usage: sp [task]

Tasks:
  --read-inline INLINE          Read inline events into ledger
  --read-file FILE              Read events in file into ledger
  --nft TOKENID                 Display owner of TOKENID
  --wallet ADDRESS              Display contents of wallet ADDRESS
  --reset                       Reset the ledger
";
}