package main

import "github.com/emicklei/go-restful"

type WinofyRouting interface {
	Route(ws *restful.WebService)
}
