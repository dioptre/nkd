/*jshint browser:true */
/*globals deepEqual equal ok assert test module */
(function() {
	"use strict";
	module("Encoding");

	/**
	* Serialize an Element to a string easily
	*/
	var s = (function() {
		var serializer = new XMLSerializer();
		return function(node) {
			return serializer.serializeToString(node);
		};
	})();

	/**
	* A wrapper around test functions that makes an Mkel function for testing
	* with. Kinda like a Python decorator
	*/
	var needMkel = function(fn) {
		return function() {
			var doc = document.implementation.createDocument(null, null, null);
			var mkel = function(name, children) {
				var node = doc.createElement(name);
				if (arguments.length == 1) return node;

				if (typeof children === 'string') {
					node.appendChild(doc.createTextNode(children));
				} else if (Array.isArray(children)) {
					children.forEach(node.appendChild.bind(node));
				} else if (children instanceof Element) {
					node.appendChild(children);
				} else {
					console.log('Bad arguments', arguments, children instanceof String, typeof children);
					throw new Error("Unknown type supplied to `mkel`");
				}
				return node
			};

			var args = [].slice.call(arguments);
			args.unshift(mkel);

			return fn.apply(this, args);
		};
	};

	test("JavaScript primitive value encoding", needMkel(function(mkel) {
		var types = XMLRPC.types;

		deepEqual(types.boolean.encode(true, mkel), mkel('boolean', '1'),
			"Boolean true encodes to <boolean>1</boolean>");

		deepEqual(types.boolean.encode(false, mkel), mkel('boolean', '0'),
			"Boolean true encodes to <boolean>0</boolean>");


		deepEqual(types.int.encode(3, mkel), mkel('int', '3'),
			"Integer 3 encodes to <int>3</int>");
		deepEqual(types.i8.encode(4, mkel), mkel('i8', '4'),
			"Integer 3 encodes to <i8>4</i8>");
		deepEqual(types.double.encode(5.5, mkel), mkel('double', '5.5'),
			"Double 5.5 encodes to <double>5.5</double>");

		deepEqual(types.nil.encode(null, mkel), mkel('nil'),
			"Null encodes to <nil>");
		deepEqual(types.nil.encode("hello", mkel), mkel('nil'),
			"Null encodes to <nil> when supplied a non-null value");

		deepEqual(types.string.encode("Hello, World!", mkel), mkel('string', "Hello, World!"),
			"String encodes to <string>...</string>");
		deepEqual(types.string.encode("", mkel), mkel('string', ""),
			"Empty String encodes to <string></string>");

		var timestamp = 1350943077107;
		var datestring = "2012-10-22T21:57:57Z";
		var date = new Date();
		date.setTime(timestamp);
		deepEqual(types['date.iso8601'].encode(date, mkel), mkel('date.iso8601', datestring),
			"Date encodes to <date.iso8601>...</date.iso8601>");

	}));

	test("Array encoding", needMkel(function(mkel) {
		var types = XMLRPC.types;

		equal(s(types.array.encode([4, "Hello"], mkel)),
			'<array><data><value><int>4</int></value><value><string>Hello</string></value></data></array>',
			"Simple array encodes");

		// If not all browsers encode this to <data/>, this will fail.
		equal(s(types.array.encode([], mkel)),
			'<array><data/></array>',
			"Empty array encodes");

		equal(s(types.array.encode([1, [2]], mkel)),
			'<array><data>' +
				'<value><int>1</int></value>' +
				'<value><array><data>' +
					'<value><int>2</int></value>' +
				'</data></array></value>' +
			'</data></array>',
			"Array containing array encodes");
	}));

	test("Guessing types", needMkel(function(mkel) {
		equal(XMLRPC.toXMLRPC(4, mkel).nodeName, 'int',
			"Number 4 guessed to be <int>");

		equal(XMLRPC.toXMLRPC(4.5, mkel).nodeName, 'double',
			"Number 4.5 guessed to be <double>");

		equal(XMLRPC.toXMLRPC(true, mkel).nodeName, 'boolean',
			"Boolean guessed to be <boolean>");

		equal(XMLRPC.toXMLRPC(null, mkel).nodeName, 'nil',
			"null guessed to be <nil>");

		equal(XMLRPC.toXMLRPC(undefined, mkel).nodeName, 'nil',
			"undefined guessed to be <nil>");

		equal(XMLRPC.toXMLRPC("Hello", mkel).nodeName, 'string',
			"String guessed to be <string>");

		equal(XMLRPC.toXMLRPC(new Date(), mkel).nodeName, 'date.iso8601',
			"Date guessed to be <date.iso8601>");

		equal(XMLRPC.toXMLRPC({foo: 'bar'}, mkel).nodeName, 'struct',
			"Object guessed to be <struct>");

		equal(XMLRPC.toXMLRPC([], mkel).nodeName, 'array',
			"Array guessed to be <array>");

		equal(XMLRPC.toXMLRPC(new ArrayBuffer(), mkel).nodeName, 'base64',
			"ArrayBuffer guessed to be <base64>");
	}));

})();
