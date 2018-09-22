# filehasher

a CommandLine application to get hash value of a file in .Net Framework 2.0.
Support md5, sha1, sha256 and so on, for details see HashAlgorithm.Create() in MSDN documents.

```
Usage:
    fileshasher --file <file_path> [--alg <alg_name>]

Arguments:
    --file       file path to compute hash
    --alg        optional, name ofhash algorithm, i.e. md5(default), sha1, sha256...
```

Example:
```
> filehasher --file c:\foo.txt --alg md5
```