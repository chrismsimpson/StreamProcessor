
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

    public static int Main(String[] args) {

        switch (true) {

            case var _ when
                args.ValueForKey("--read-file") is String file: {

                WriteLine($"TODO: read inline");
                
                return 0;
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