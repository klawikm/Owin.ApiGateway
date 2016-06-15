# Owin.ApiGateway
API gateway implemented in .NET using OWIN framework.

## Motivation
One of the requirements for a good micro services architecture is that all micro services
should not call each other directly but through API Gateway component. You can learn more
about API Gateway pattern [here](http://microservices.io/patterns/apigateway.html). Despite of long
research, me and my colleagues were not able to find any reasonable solution implemented
in .NET for **on-premise** usage. Why .NET? Because I am a .NET developer and see below mentioned advantages of
having API Gateway implemented in .NET:

* Technology Consistency - all my building blocks are created in the same programming
language;
* Easy to troubleshot - as a guy with strong C# skills I can use all troubleshoting techniques I know to find bugs and identify performance bottlenecks.

There are plenty of API Gateways implemented either in other programming languages or
created by Microsoft but for some reasons not addressing my needs. You can jump to
*API Gateways comparison table* section in this README to see comparison of some available API Gateways.

## Project status

version 0.0.1 - I've just started :-)

## Desired features

* Flexibility and Extensibility
* Routing / balancing
* Service Registry
* Wide logging options (audit logs, including requests and responses)
* Wide caching options (including POST operations)
* Circuit Breaker
* Health checking / monitoring

## Implemented features

* Static routing of SOAP based and RESTfull web-services
* Routing based on SOAP Action header or request path
* Routing configuration stored in XML or YAML file
* API Gateway can run as "self host" but also can be hosted in IIS
* Response caching (currently only via MemoryCache but you can also inject your own cache provider) 
* Configuration can be stored in central store e.g. in SQL database. This solution is required when API Gateway is running in cluster
* WEB API providing CRUD operations for configuration management
* Configuration synchronization between nodes in the cluster (Publish-Subscribe pattern implemented using Rebus: https://github.com/rebus-org/Rebus)
* Load balancing (round robin)
* Health checking / monitoring

## Road map

1. To implement Circuit Breaker. To learn more about circuit breaker pattern go to (http://martinfowler.com/bliki/CircuitBreaker.html)
2. To implement efficient audit logs. It will be possible to save request and response body to database
3. To implement real time monitoring. Solution will send time statistics to time series database e.g. to [Graphite](https://github.com/graphite-project/graphite-web) via [Stastd](https://github.com/etsy/statsd). This will make possible to build monitoring dashboards using [Grafana](http://grafana.org/) tool.
4. [Azure Service Fabric](https://azure.microsoft.com/en-us/services/service-fabric/) integration. As far as I know there is no available API Gateway solution that acts as access point to Web APIs hosted in Service Fabric.
5. Integration with OAuth2 (authorization proxy)

## Possible use-cases

* API Gateway in micro services architectures
* Access point to Web APIs hosted in Service Fabric (on-premise)

## API Gateways comparison table

|Feature| IIS + ARR | Service Fabric | Netflix (linux, JAVA) | NGINX (linux, C) |
|-------| --------- | -------------- | --------------------- | -----------------|
|Transport| HTTP(s),  Terminating TLS/SSL, GZIP compression | Does not matter | Terminating TLS/SSL, GZIP compression | Terminating TLS/SSL, GZIP compression |
| REST | yes | yes | yes | yes |
| SOAP | yes | yes | yes | yes |
| Routing | based on URL regex match, based on HTTP headers, session persistence (cookie), load balancing (multiple algorithms) | load balancer is not included | yes, mature with a lot of options, by filters, by ribbon (client loadbalancer) by eureka (service registry) |  based on application parameters, A/B testing (simple rule to split traffic into two parts), session persistence (cookie, sticky, scripted), session draining, load balancing (TCP/HTTP, connection limit, rate limits (requests per second and requests per minute), methods: Round-Robin, Least Connections, Generic Hash, and IP Hash, Least Time) |
| Configuration via REST API | no | yes | yes | yes |
| PowerShell support | yes | yes | no | no |
| Service Registry | no | yes | yes, eureka service, easy to integrate | Partial (via "Consul Template to dynamically reconfigure NGINX reverse proxying") |
| Health checking| yes | yes | yes | yes (based on URI, regex response validation, slow-start) |
| Logging | partial. Logging full requests/responses to DB not possible out of the box | no | yes, by filters, but also some OSS products | only standard HTTP access log formats |
| Authorization | yes (URL Authorization) | no | yes, by filters | partial (based on IP or flat file with usernames and passwords) |
| Caching| yes for GET operations, not possible for POST operations | no | yes by filters, by ribbon | yes (internal or external/Redis) |
| Circuit Breaker | no | no | yes, by Hystrix library, looks like very mature solution, but also looks is only applicable for Java applications | no |
| Monitoring dashboards | no | yes | yes | yes (seems very nice and there is also REST API for external integration) |
| Extensibility | yes (custom modules) | yes | fully, thanks to plugin architecture | possiblie with C, static compilation*, probably Lua langugae as well |
| Recommended OS| Windows | Windows | Linux | Linux |
| Price | included in OS price | free | free | basic version for free |
