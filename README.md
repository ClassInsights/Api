# ClassInsights API

> ASP.NET Core API to connect all WinService instances with the ClassInsights Server

## Overview

ClassInsights API is a RESTful backend service built with ASP.NET Core that serves as the central communication hub for the ClassInsights ecosystem. It manages data synchronization between Windows service instances and provides a unified interface for the ClassInsights platform.

## Features

- **JWT Authentication** - Secure bearer token-based authentication
- **PostgreSQL Database** - Entity Framework Core with NodaTime support for timezone-aware operations
- **Untis Integration** - Automated data synchronization with Untis
- **Rate Limiting** - Built-in rate limiting (120 requests/minute per user)
- **LDAP Support** - Directory Services integration via System.DirectoryServices.Protocols
- **Docker Support** - Production-ready containerization with Linux-based images
- **API Documentation** - Interactive Scalar API reference (development mode)
- **Automatic Migrations** - Database schema updates on production deployment
- **WebSocket Support** - Real-time communication capabilities

## Tech Stack

- **.NET 10.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM with PostgreSQL provider
- **NodaTime** - Timezone-aware date/time handling
- **Docker** - Containerization
- **JWT Bearer** - Authentication mechanism

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) database
- Docker (optional, for containerized deployment)

## Getting Started

We offer a [Demo](https://classinsights.at/demo) for every school that wants to try out ClassInsights.