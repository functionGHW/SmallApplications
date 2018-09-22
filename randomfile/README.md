# randomfile

a CommandLine application to create temp file with special size, and fill it with random bytes.

```
Usage:
    randomfile --size <file_size> [--filename <file_name>]

Arguments:
    --size       file size to generate, default unit is B(yte), , i.e. 512, 1000B, 200MB
    --filename   optional, output filename, default is randomfile.tmp
```

Example:
```
> randomfile --size 512MB --filename c:\foo\bar.tmp
```