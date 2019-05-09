package main

import (
	"crypto/rand"
	"fmt"
	"github.com/emicklei/go-restful"
	"golang.org/x/crypto/bcrypt"
	"log"
	"time"
)

type LoginRouting struct {

}

func (login *LoginRouting) Route(ws *restful.WebService) {
	ws.Route(ws.POST("/login").To(login.registerFunc))
}

func (login *LoginRouting) registerFunc(request *restful.Request, response *restful.Response) {
	name, err := request.BodyParameter("username")

	//FIXME variable "name" can inject SQL

	if err != nil {
		response.WriteHeader(400)
		log.Println(err)
		return
	}

	pass, err := request.BodyParameter("password")

	if err != nil {
		response.WriteHeader(400)
		log.Println(err)
		return
	}

	q := "SELECT Username, Password FROM Users WHERE Username = '" + name + "'"
	rows, err := sqlConnection.Query(q)

	if err != nil {
		response.WriteHeader(400)
		log.Println(err)
		return
	}

	rname, hashed := "", ""
	defer rows.Close()

	if !rows.Next() {
		response.WriteHeader(200)
		return
	}

	err = rows.Scan(&rname, &hashed)

	if err != nil {
		response.WriteHeader(400)
		log.Println(err)
		return
	}

	if login.comparePasswords(hashed, []byte(pass)) {
		token, err := login.setOrGetToken(name)

		if err != nil {
			response.WriteHeader(500)
			log.Println(err)
			return
		}

		response.Write([]byte(token))
		return
	}

	response.WriteHeader(200)
}

func (login *LoginRouting) setOrGetToken(username string) (string, error) {
	q := "SELECT Username, Creation, Token FROM Tokens WHERE Username = '" + username + "'"
	rows, err := sqlConnection.Query(q)
	defer rows.Close()

	if err != nil {
		return "", err
	}

	var token string
	var creation time.Time
	now := time.Now()

	for rows.Next() {
		rows.Scan(&username, &creation, &token)

		//Token expires in 72 hours
		if now.Sub(creation).Hours() <= 72 {
			return token, nil
		}
	}

	token = login.generateToken()
	exec := "INSERT INTO Tokens (Username, Creation, Token) VALUES ('" + username + "', NOW(), '" + token + "')"
	_, err = sqlConnection.Exec(exec)
	log.Println("Inserted new token for " + username)

	return token, err
}

func (login *LoginRouting) generateToken() string {
	b := make([]byte, 50)
	rand.Read(b)
	return fmt.Sprintf("%x", b)
}

func (login *LoginRouting) comparePasswords(hashedPwd string, plainPwd []byte) bool {
	// Since we'll be getting the hashed password from the DB it
	// will be a string so we'll need to convert it to a byte slice
	byteHash := []byte(hashedPwd)
	err := bcrypt.CompareHashAndPassword(byteHash, plainPwd)

	if err != nil {
		log.Println(err)
		return false
	}

	return true
}
