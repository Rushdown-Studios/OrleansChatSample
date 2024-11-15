.PHONY:base

server:
	@echo "Setting up services..."
	docker-compose up -d

shutdown:
	@echo "Shutting down services..."
	docker-compose down

