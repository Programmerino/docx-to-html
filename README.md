# [DOCX To HTML](https://docxtohtml.0bit.dev/)

Powered by .NET using Bolero and Clippit . This is the best method I found for converting docx to HTML thus far that's viable for client-side execution.

## Notes

* It uses the old .NET Core 3.1 instead of 5.0 because of some weird GC bug that appears to be a regression (I haven't had time to file this yet, but it's ```Assertion: should not be reached at sgen-scan-object.h:91```). The new version also seems to run out of memory quicker with rudimentary testing, so I guess that's a silver lining.
  * In doing so, there is a workaround function relating to lower maximum URI limits in the old .NET version

## CORS Error
This is powered by IPFS, and if you are using the IPFS Companion extension, you either need to whitelist the website or use Firefox otherwise the website's resources are separated and seen as being from different origins