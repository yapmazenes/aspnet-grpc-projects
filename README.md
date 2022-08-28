# Using gRPC in Microservices for Building a high-performance Interservice Communication with .Net 5

### Overall Picture

Real-world **e-commerce microservices** project. 6 microservices.
Worker Services and Asp.Net 5 Grpc applications to build client and server gRPC components defining proto service definition contracts.

![Overall Picture of Repository](https://user-images.githubusercontent.com/1147445/98652230-5f66ee80-234c-11eb-9201-8b291b331c9f.png)

- Basically implemented e-commerce logic with only gRPC communication. Have 3 gRPC server applications which are Product — ShoppingCart and Discount gRPC services. 
- 2 worker services which are Product and ShoppingCart Worker Service.
- Worker services will be client and perform operations over the gRPC server applications.
- System secured the gRPC services with standalone Identity Server microservices with OAuth 2.0 and JWT token.

I Have implemented this base project from Using gRPC in Microservices Communication with .Net 5 - [mehmetozkaya](https://github.com/mehmetozkaya)
