
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

    public static int Main(String[] args) {

        switch (true) {

            case var _ when
                args.ValueForKey("--read-file") is String filename: {

                if (!File.Exists(filename)) {

                    WriteErrorLine($"file '{filename}' does not exist");

                    return 1;
                }

                var itemsOrError = GetStreamItemsFromFilename(filename);

                if (itemsOrError.Error is not null
                    || itemsOrError.Value is null) {

                    WriteErrorLine(itemsOrError.Error?.Content ?? "unknown error");

                    return 1;
                }

                return 0;
            }

            case var _ when
                args.ValueForKey("--read-inline") is String inline: {

                var itemsOrError = GetStreamItemsFromContents(inline);

                if (itemsOrError.Error is not null
                    || itemsOrError.Value is null) {

                    WriteErrorLine(itemsOrError.Error?.Content ?? "unknown error");

                    return 1;
                }

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