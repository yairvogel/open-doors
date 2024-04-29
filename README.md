# open-doors
An API service that allows users to open doors.

## Running
1. Make sure you have dotnet installed. [link](https://dotnet.microsoft.com/en-us/download)
2. Install entity framework core `dotnet tool install --global ef`
3. The service relies on a sqlite database. create it using `dotnet ef database update --project OpenDoors.Service`
4. Run the service with `dotnet run --project OpenDoors.Service`


## summary
This is a sample api allows users to open doors using an app or straight up api. 
The api is a multi tenant app, backed a (sqlite) database, that exposes a door 
opening api, a log api and some related management apis.

## Functional Requirements
- Allow to open doors
- Show a log of attempts at opening doors
- Allow entry only for authorized personnel

## Design choices
I assumed that there's an external system that is responsible for opening doors 
and there's a separate flow for onboarding new doors. I modeled the door opening
mechanism with an unimplemented interface - where probably there's a separate
module that implements that interface when actual integration is made.

Therefore, the main consideration is about access control - managing and
identifying who can access which door and trigger that door opening mechanism.
That requires an identity and authorization systems to manage that access. The 
management of hundreds of doors or even thousands with potentially thousands of
users is made very complicated quite easily, mostly for the user that tries to
reason about his access policy.

I took inspiration from virtual access control for cloud environments and used
**access groups** as a method of managing users and doors. The access group is
an entity that holds multiple users, where doors can allow access to one or more
groups.

For example: assume I'm a hotel manager that wants to grant permission to guests
that stay at the fourth floor, at room 414. The guests need access to the room,
as well as the floor general door. I'll define an access group called 'Room 414'
and add the room lock for 414 and the general 4th floor key. When the guests
check in they are granted access to the access group 'Room 414' which
automatically gives them access to both doors. The group membership can also
have an expiry date to deny the guests access after they leave (a feature that
is not implemented but can easily extend to support). 'Room 415' access group 
will behave similarly, referencing the same general 4th floor door.

### Database design
The database uses the following tables:
- Users
- Roles
- Doors
- Access Groups
- Tenants
- Log

The users and roles are simple IdentityUser/Role with the extension of a FK to
Tenants (as they belong to one tenant).
Doors are an entity that hold simple information about door entities.
There's a table AccessGroupDoors that supports a many-to-many relationship
between access groups to doors. If there's a link, the door allows access to
members of the access group.
There's a table AccessGroupMemberships that supports many-to-many relationship
between users and access groups. This entity can also have an expiry date to
automatically disallow users from accessing once the membership is past-due.
Access groups belong to only one tenant, and can be queried using the tenant id
as well.
EntryLogs are entities that document door opening attempts, to be available for
querying. They hold door and user keys, but are not defined as a FK constraint
to decouple them from the actual entity (if a door is deleted, we still want
its history).

### Design concerns
- Adding an indirection between users and doors can make it difficult perform
three-way joins on the tables when data grows. The workload is designed that
long join chains are filtered and reduced first, narrowing the working set of
the join.
- The API works with sqlite, but can easily evolve into a more scalable database
- The log currently is a synchronous insertion to the database. The database is
currently used in a distinct OLTP workload, and the log insertion and fetching
might benefit from a database configured more towards OLAP. We can compromise
consistency of the log information for higher performance
