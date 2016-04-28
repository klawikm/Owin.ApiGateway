# Owin.ApiGateway
API gateway implemented in .NET using OWIN framework.

## Motivation
One of the requirements for a good micro services architecture is that all micro services
should not call each other directly but through API Gateway component. You can learn more
about API Gateway pattern [here](http://microservices.io/patterns/apigateway.html). Despite of long
research, me and my colleagues were not able to find any reasonable solution implemented
in .NET for **on-premise** usage. Why .NET? Because I am a .NET developer and see below mentioned advantages of
having API Gateway implemented in .NET:
1. Technology Consistency - all my building blocks are created in the same programming
language;
2. Easy to troubleshot - as a guy with strong C# skills I can use all troubleshoting techniques I know to find bugs and identify performance bottlenecks.

There are plenty of API Gateways implemented either in other programming languages or
created by Microsoft but for some reasons not addressing my needs. You can jump to
*API Gateways comparison table* section in this README to see comparison of some available API Gateways.

## Project status

version 0.0.1 - I've just started :-)

## Implemented features

* Static routing of SOAP based and RESTfull web-services
* Routing based on SOAP Action header or request path
* Routing configuration stored in XML or YAML file

## Road map

1. To implement caching. It means that for certain period of time API Gateway will not forward requests to the backend and will serve responses from cache. It will work also for SOAP based web-services (HTTP POST);
2. To implement Service Registry. Use case: new instance of web-service registers itself in
Service Registry or is registered in Service Registry by someone else. Service Registry is then used to implement **dynamic load balancing**.
3. To implement Circuit Breaker. To learn more about circuit breaker pattern go to (http://martinfowler.com/bliki/CircuitBreaker.html)
4. To implement efficient audit logs. It will be possible to save request and response body to database
5. To implement real time monitoring. Solution will send time statistics to time series database e.g. to [Graphite](https://github.com/graphite-project/graphite-web) via [Stastd](https://github.com/etsy/statsd). This will make possible to build monitoring dashboards using [Grafana](http://grafana.org/) tool.
6. [Azure Service Fabric](https://azure.microsoft.com/en-us/services/service-fabric/) integration. As far as I know there is no available API Gateway solution that acts as access point to Web APIs hosted in Service Fabric.
7. Integration with OAuth2 (authorization proxy)

## Possible use-cases

* API Gateway in micro services architectures
* Access point to Web APIs hosted in Service Fabric (on-premise)

## API Gateways comparison table

|Feature| IIS + ARR | Service Fabric | Netflix (linux, JAVA) | NGINX (linux, C) |
|-------| --------- | -------------- | --------------------- | -----------------|
|Transport| HTTP(s),  Terminating TLS/SSL, GZIP compression | Does not matter | ??? | Terminating TLS/SSL, GZIP compression |
| REST | yes | yes | yes | yes |
| SOAP | yes | yes | yes | yes |
| Routing | based on URL regex match, based on HTTP headers, session persistence (cookie), load balancing (multiple algorithms) | load balancer is not included | ??? | ??? |
| Configuration via REST API | no | yes | ??? | ??? |
| PowerShell support | yes | yes | ??? | ??? |
| Service Registry | no | yes | ??? | ??? |
| Health checking| yes | yes | ??? | ??? |
| Logging | partial. Logging full requests/responses to DB not possible out of the box | no | ??? | ??? |
| Authorization | yes (URL Authorization) | no | ??? | ??? |
| Caching| yes for GET operations, not possible for POST operations | no | ??? | ??? |
| Circuit Breaker | no | no | ??? | ??? |
| Monitoring dashboards | no | yes | ??? | ??? |
| Extensibility | yes (custom modules) | yes | ??? | ??? |
| Recommended OS| Windows | Windows | ??? | ??? |
| Price | included in OS price | free | free | basic version for free |
