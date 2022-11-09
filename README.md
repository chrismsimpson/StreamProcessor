# Stream Processor

The stream processor has essentially three steps:

- Serialization via the `StreamItem` abstraction. This is the way both input files/content are read into the ledger as well as the transport the ledger itself is serialized using.

- Transformation into the `IStreamEvent`s `Mint`, `Burn` & `Transfer`. 

- Verification of incoming events that, if valid, are added to the stream (which in turn is re-serialized).

Therefore, the ledger is passed around as both a stream of items and a valid dictionary (as a Tuple). Transfers, for example, must already be owned by the correct address in order to be applied.

Serialization to/from disk is done via System.Text.Json. This is a quick and dirty implementation, albiet still reasonbly performant. One might want to create a custom JSON lexer in order to only ever do a single pass of an incoming stream, and to align error handling.

On error handling, no exceptions are used. Instead, The functional abstractions of `Error/Error?` and `ErrorOr<Result>` are used. This allows all failure scenarios to 'bubble up' to the caller.

I have also specified an override for the ledger file location using `--ledger FILE`. The default is stored at `./Store/ledger.json`, which is created on first run if need be.