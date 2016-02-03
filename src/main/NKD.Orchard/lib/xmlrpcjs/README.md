JavaScript XML-RPC library
======================

This is a small library for communicating with XML-RPC services - without
worrying about the horrible bloat of XML-RPC. Using this library, you can pass
JSON parameters to the library, and receive responses in JSON. Encoding the
JSON document is handled for you, intelligently mapping types between the two
languages.

Installing
----------

Simply include this library in your page:

```html
<script src="xmlrpc.js"></script>
```

This was built using ES5 features. It will probably work with shims for old
browsers.

Using
-----

The `XMLRPC.XMLRPCRequest` class is the main work-horse of this library.
It is a wrapper around the XMLHttpRequest class that works with XML-RPC
documents. Use it like so:

```javascript
TODO
```

It takes all of the same arguments as `XMLHttpRequest`, so refer there for more
documentation. Upon completion of a request, the XMLRPCRequest instance will
have one extra parameter, `responseJSON`.

See the docs section on [Encoding and Decoding XML-RPC Documents][encoding] for
more information on how types are encoded

### Handling errors

TODO

Documentation
-------------

[The full documentation can be found on Read The Docs][docs].

[docs]: http://javascript-xml-rpc.readthedocs.org/ "Documentation"
[encoding]: http://javascript-query-xml-rpc.readthedocs.org/en/latest/types.html#encoding-and-decoding-xml-rpc-documents
	"Encoding and Decoding XML-RPC Documents"
