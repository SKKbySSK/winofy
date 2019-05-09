package main

import (
	"github.com/emicklei/go-restful"
	"golang.org/x/crypto/bcrypt"
	"log"
)

type RegisterRouting struct {

}

func (register *RegisterRouting) Route(ws *restful.WebService) {
	ws.Route(ws.PUT("/register").To(register.registerFunc))
}

func (register *RegisterRouting) registerFunc(request *restful.Request, response *restful.Response) {
	name, err := request.BodyParameter("username")

	//FIXME variable "name" can inject SQL

	if err != nil {
		response.WriteHeader(400)
		log.Println(err)
		return
	}

	if len(name) < 3 || len(name) > 40 {
		response.WriteHeader(400)
		return
	}

	pass, err := request.BodyParameter("password")

	if err != nil {
		response.WriteHeader(400)
		log.Println(err)
		return
	}

	if len(pass) < 5 || len(pass) > 40 {
		response.WriteHeader(400)
		return
	}

	rows, err := sqlConnection.Query("SELECT Username FROM Users WHERE Username = \"" + name + "\"")
	defer rows.Close()

	if err != nil {
		response.WriteHeader(400)
		log.Println(err)
		return
	}

	if rows.Next() {
		response.WriteHeader(409)
		return
	}

	hashed := register.hashAndSalt([]byte(pass))
	exec := "INSERT INTO Users (Username, Password, Creation) " +
		"VALUES ('" + name + "', '" + hashed + "', NOW())"
	_, err = sqlConnection.Exec(exec)

	if err != nil {
		response.WriteHeader(500)
		log.Println(err)
		return
	}

	response.WriteHeader(200)
}

func (register *RegisterRouting) hashAndSalt(pwd []byte) string {

	// Use GenerateFromPassword to hash & salt pwd.
	// MinCost is just an integer constant provided by the bcrypt
	// package along with DefaultCost & MaxCost.
	// The cost can be any value you want provided it isn't lower
	// than the MinCost (4)
	hash, err := bcrypt.GenerateFromPassword(pwd, bcrypt.DefaultCost)
	if err != nil {
		log.Println(err)
	}
	// GenerateFromPassword returns a byte slice so we need to
	// convert the bytes to a string and return it
	return string(hash)
}