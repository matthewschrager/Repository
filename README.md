Repository
=============

Repository is a generic implementation of the Repository pattern in C#. It provides a repository interface ```IRepository<T>``` that exposes functions to store/retrieve data,
and an object context interface ```IObjectContext<T>``` that enables manipulation of data once it's retrieved. It also exposes a ```GetItemsContext``` method that returns
an ```IQueryable<T>``` which can be used to perform LINQ queries on the repository.

Implementations
================

This (code) repository comes with a few Repository implementations. The first and simplest is ```InMemoryRepository```, which acts as a temporary in-memory store useful mainly for testing. The second is
an Entity Framework repository named ```EFRepository```, which uses [Entity Framework](http://msdn.microsoft.com/en-us/data/ef.aspx) as its storage interface. The last (and most extensively developed) is ```RavenRepository```, 
a [RavenDB](http://ravendb.net/) implementation.

Pull requests for additional implementations (and improvements to existing ones) are welcome and encouraged.

Examples
===========

Store an object:
-----------------

```C#
var repository = new MyConcreteRepository<int>();
repository.Store(1); // Store a single value
repository.Store(new[] { 1, 2, 3, 4, 5 }); // Store a bunch of values
```

Retrieve an object:
--------------------

```C#
class MyClass 
{
	public String Key { get; set; }
}

/* ... */

var repository = new MyConcreteRepository<MyClass>();

// Objects are returned inside of their own ObjectContexts, which should be disposed
using (var valueContext = repository.Find("myKey"))
{ 
	var value = valueContext.Object;

	// Do cool things with this value
}
```

Query stored objects:
----------------------

```C#
class MyClass
{
	public String Key { get; set; }
	public int Value { get; set; }
}

/* ... */

var repository = new MyConcreteRepository<MyClass>();
using (var itemsContext = repository.GetItemsContext())
{
	// Query objects based on their Value field
	var filteredItems = itemsContext.Where(x => x.Value > 5);

	// Do stuff with query result
}
```


Remove an object:
-----------------

```C#
class MyClass 
{
	public String Key { get; set; }
}

/* ... */

var repository = new MyConcreteRepository<MyClass>();
repository.Remove("myKey");
```

Modify a stored object:
-----------------------

```C#
class MyClass 
{
	public String Key { get; set; }
	public int Value { get; set; }
}

/* ... */

// Pull the object from the repository
var repository = new MyConcreteRepository<MyClass>();
using (var valueContext = repository.Find("myKey"))
{
	// Modify it
	valueContext.Object.Value = 5;

	// Save the changes
	valueContext.SaveChanges();
}
```

License
===========

'''
Copyright (c) 2012, Matthew Schrager
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of Matthew Schrager nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL MATTHEW SCHRAGER BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
'''









