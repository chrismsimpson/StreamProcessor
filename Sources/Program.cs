
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

    public static int Main(String[] args) {

        switch (true) {

            case var _ when
                args.ValueForKey("--read-file") is String file: {

                if (!File.Exists(file)) {

                    WriteErrorLine("file '{file}' does not exist");

                    return 1;
                }

                var contents = File.ReadAllText(file);

                var peek = contents.FirstOrDefault();

                switch (peek) {

                    case '[': {

                        var items = JsonSerializer.Deserialize<List<StreamItem>>(contents);

                        return 0;
                    }

                    case '{': {

                        var item = JsonSerializer.Deserialize<StreamItem>(contents);

                        return 0;
                    }

                    default: {

                        WriteErrorLine("'{file}' does not appear to be valid json");

                        return 1;
                    }
                }
                

                // WriteLine($"TODO: read inline");
                
                // return 0;
            }

            case var _ when
                args.ValueForKey("--read-inline") is String inline: {

                WriteLine($"TODO: read inline");

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