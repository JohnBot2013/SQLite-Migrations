# SQLite-Migrations
## Manage Database changes in your app with SQLite

This utility depends on the wonderful SQLite library [SQLite-net](https://github.com/praeclarum/sqlite-net).

Any database application needs a way to maintain its schema. With Entity Framework you have a "Migrations" tool built in to Visual Studio. With SQLite then there is not a proper way to maintain your database schema with changes tied to the application version.

This utility provides a way to keep your database in line with your code base automatically, every time your application starts.

Each set of changes is captured using an IMigration. Each Migration must have a 3 digit sequential number at the end of its name. When each Migration is executed, the number becomes the current application version and is stored using a SQLite pragma.

The Migration process is very efficient, Migrations are only executed if their number is higher than the current version.

Each Migration can provide as complex a set of changes as needed and includes seeding.

The utility provides a BaseMigration for you to use. This has a number of utilities which are helpful when making column of data changes.

The unit tests provide some examples of usage.