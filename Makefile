.PHONY: build-push-image
build-push-image:
	docker build . -t ghcr.io/exit-path/server:latest -t ghcr.io/exit-path/server:$(shell git rev-parse HEAD)
	docker push ghcr.io/exit-path/server:latest
	docker push ghcr.io/exit-path/server:$(shell git rev-parse HEAD)
