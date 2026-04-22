cmake_minimum_required(VERSION 3.26)
project(KingsHandsProject)

set(CS_PROJECT "${CMAKE_SOURCE_DIR}/KingsHandsProject/KingsHandsProject.csproj")

add_custom_target(KingsHandsProject ALL
    COMMAND dotnet build ${CS_PROJECT} -c Release
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
)