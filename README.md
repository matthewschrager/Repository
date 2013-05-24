Repository
=============

Repository is a generic implementation of the Repository pattern in C#. It provides a repository base class ```Repository<T>``` that exposes functions to store/retrieve data,
and an object context base class ```ObjectContext<T>``` that enables manipulation of data once it's retrieved. It also exposes an ```Items``` property that returns 
an ```IQueryable<T>``` which can be used to perform LINQ queries on the repository.

Implementations
================

This (code) repository comes with a few Repository implementations. The first and simplest is ```InMemoryRepository```, which acts as a temporary in-memory store useful mainly for testing. The second is
an Entity Framework repository named ```EFRepository```, which uses [Entity Framework](http://msdn.microsoft.com/en-us/data/ef.aspx) as its storage interface. The last is ```AzureRepository```, 
an implementation of ```Repository``` for [Azure Blob Storage](http://www.windowsazure.com/en-us/manage/services/storage/).

Pull requests for additional implementations (and improvements to existing ones) are welcome and encouraged.

Examples
===========

Store an object:
-----------------

```C#
var repository = new MyConcreteRepository<int>();
repository.Store(1); // Store a single value
repository.SaveChanges();


repository.Store(new[] { 2, 3, 4, 5 }); // Store a bunch of values
repository.SaveChanges();
```

Retrieve an object:
--------------------

```C#
class MyClass 
{
	public String Key { get; set; }
}

/* ... */

using (var repository = new MyConcreteRepository<MyClass>())
{
	var valueContext = repository.Find("myKey"))
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

using (var repository = new MyConcreteRepository<MyClass>())
{
	// Query objects based on their Value field
	var filteredItems = repository.Items.Where(x => x.Value > 5);

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

using (var repository = new MyConcreteRepository<MyClass>())
{
	repository.RemoveByKey("myKey");
	repository.SaveChanges();

	// Or...

	var obj = new MyClass { Key = "myKey" };
	repository.Remove(obj);
	repository.SaveChanges();

	// Or...

	repository.RemoveByKey("myKey");
	repository.SaveChanges();

	// Or...

	repository.RemoveAllByKey("myKey");
	repository.SaveChanges();

	// Or...
	repository.RemoveAll();
	repository.SaveChanges();
}
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
using (var repository = new MyConcreteRepository<MyClass>())
{
	// Pull the object from the repository
	var valueContext = repository.Find("myKey");

	// Modify it
	valueContext.Object.Value = 5;

	// Save the changes
	repository.SaveChanges();
}
```

License
===========

Repository is licensed under the New BSD License:

```
Copyright (c) 2013, Matthew Schrager
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
```









