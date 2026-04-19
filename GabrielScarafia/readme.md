
En el archivo "Danaide.postman_collection"
se encuentra la coleccion de postman que utilice para las pruebas de la API, se pueden importar a postman para probar

El proyecto se encuentra desarrollado en .NET 10 y documentado con OpenApi y Scalar.

La api se realizo en una arquitectura de vertical slice, aunque no pura, debido al desacoplamiento entre la capa de API y la logica del negocio.

Para la persistencia de datos se utilizo una base de datos en memoria, con SQLLite, aunque se configuro para que se pueda cambiar facilmente a otra base de datos, como MySQL. 
Hay funciones que se encargan de crear la base de datos y las tablas necesarias, y otras que se encargan de insertar datos de prueba.

para facilitar la lectura se genero unos endpoints de Debug, que se encargan de mostrar la informacion de la base de datos, 
se encuentran ejemplos del uso de los endpoints en la coleccion de postman.

Dejo ejemplos de usuarios que use:
Usuario VIP
{
    "Id": 11,
    "Nombre": "Gabi",
    "Clave": "Gabi",
    "Dni": "42155265",
    "Vip": 1
}
Usuario Comun
{
    "Id": 10,
    "Nombre": "Rosa",
    "Clave": "Rosa",
    "Dni": "39012345",
    "Vip": 0
}

En la base se encuentra pre cargados:
Usuarios: 11
Carros: 11 (1 por usuario, cada carro se encuentra pre cargado con 15 productos randoms)
Productos: 20